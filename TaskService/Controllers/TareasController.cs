using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TaskService.Data;
using TaskService.DTOs;
using TaskService.Interfaces;
using TaskService.Models;

namespace TaskService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly ITareaRepository _repository;
        private readonly IMessageProducer _messageProducer;
        private readonly IFtpService _ftpService;
        private readonly IUsuarioValidator _usuarioValidator;

        public TareasController(ITareaRepository repository, IMessageProducer messageProducer, IFtpService ftpService, IUsuarioValidator usuarioValidator)
        {
            _repository = repository;
            _messageProducer = messageProducer;
            _ftpService = ftpService;
            _usuarioValidator = usuarioValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tareas = await _repository.GetAllTareasAsync();
            return Ok(tareas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tarea = await _repository.GetTareaByIdAsync(id);
            if (tarea == null) return NotFound();
            return Ok(tarea);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Tarea tarea)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _repository.CreateTareaAsync(tarea);
            return CreatedAtAction(nameof(GetById), new { id = tarea.Id }, tarea);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TareaUpdateDto dto)
        {            
            if (!Enum.TryParse<EstadoTarea>(dto.EstadoTarea, ignoreCase: true, out var estadoEnum))
                return BadRequest(new
                {
                    estadoTarea = "El estado no es válido. Use: Backlog, Doing, InReview, Done."
                });

            var tarea = await _repository.GetTareaByIdAsync(id);

            if (tarea == null)
                return NotFound($"No se encontró la tarea con ID {id}.");

            tarea.CodigoTarea = dto.CodigoTarea;
            tarea.Descripcion = dto.Descripcion;
            tarea.Titulo = dto.Titulo;
            tarea.CriteriosAceptacion = dto.CriteriosAceptacion; 

            await _repository.UpdateTareaAsync(tarea);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteTareaAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<Tarea> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("El documento de actualización no puede estar vacío.");

            var tarea = await _repository.GetTareaByIdAsync(id);
            if (tarea == null || tarea.EstadoTarea == EstadoTarea.Done)
                return NotFound($"No se encontró la tarea con ID {id}.");

            var camposPermitidos = new[] { "/estadotarea", "/tiempodesarrollo"};

            foreach (var operacion in patchDoc.Operations)
            {
                if (!camposPermitidos.Contains(operacion.path.ToLower()))
                {
                    return BadRequest(new { error = $"No se permite modificar el campo: {operacion.path}" });
                }
            }

            patchDoc.ApplyTo(tarea, ModelState); 

            if ((tarea.EstadoTarea == EstadoTarea.Done || tarea.EstadoTarea == EstadoTarea.Doing || tarea.EstadoTarea == EstadoTarea.InReview) && tarea.UsuarioId == 0)
            {
                return BadRequest(new { error = $"Estado tarea: {tarea.EstadoTarea}. Solo se puede aplicar a tareas asignadas a un usuario"  });
            }

            if (tarea.EstadoTarea == EstadoTarea.Doing && tarea.FechaInicio == null)
            {
                tarea.FechaInicio = DateTime.UtcNow;
            }

            if (tarea.EstadoTarea == EstadoTarea.Done)
            {
                if (tarea.TiempoDesarrollo <= 0)
                {
                    return BadRequest(new
                    {
                        tiempoDesarrollo = "Cuando el estado es 'Done', el tiempo de desarrollo debe ser mayor a 0."
                    });
                }

                // Establece fecha final si aún no la tiene
                if (tarea.FechaFinalizacion == null)
                    tarea.FechaFinalizacion = DateTime.UtcNow;
            }

            if (tarea.EstadoTarea == EstadoTarea.Doing && tarea.FechaInicio == null)
            {
                tarea.FechaInicio = DateTime.UtcNow;
            }

            await _repository.UpdateTareaAsync(tarea);

            return NoContent();
        }

        [HttpPost("{id}/asignar")]
        public async Task<IActionResult> AsignarTarea(int id, [FromBody] AsignacionTareaDto asignacionDto)
        {
            var tarea = await _repository.GetTareaByIdAsync(id);
            if (tarea == null) return NotFound("Tarea no encontrada");
            if (tarea.EstadoTarea == EstadoTarea.Done)
                return BadRequest("No se puede asignar una tarea finalizada");

            tarea.UsuarioId = asignacionDto.UsuarioId;

            if (!await _usuarioValidator.UsuarioExisteAsync(asignacionDto.UsuarioId))
                return NotFound("Usuario no existe");

            await _repository.UpdateTareaAsync(tarea);

            var mensaje = new AsignacionTareaMessage
            {
                TareaId = tarea.Id,
                UsuarioId = tarea.UsuarioId,
                TituloTarea = tarea.Titulo
            };
            _messageProducer.SendMessage(mensaje);

            return NoContent();
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId)
        {
            var tareas = await _repository.GetTareasByUsuarioAsync(usuarioId);
            return Ok(tareas);
        }

        [HttpPost("importar")]
        public async Task<IActionResult> ImportarTareas([FromBody] ImportarTareasRequest request)
        {
            try
            {
                var fileContent = await _ftpService.DescargarArchivoAsync(request.NombreArchivo);

                var tareasImportadas = JsonSerializer.Deserialize<List<TareaImportada>>(fileContent);
                var erroresValidacion = ValidarTareas(tareasImportadas);

                if (erroresValidacion.Any())
                {
                    return BadRequest(new
                    {
                        Message = "Errores de validación en el archivo",
                        Errors = erroresValidacion
                    });
                }

                var tareasCreadas = await _repository.ImportarTareasAsync(tareasImportadas);

                return Ok(new
                {
                    Success = true,
                    TotalRecibidas = tareasImportadas.Count,
                    TotalCreadas = tareasCreadas,
                    Duplicadas = tareasImportadas.Count - tareasCreadas,
                    Message = $"Se importaron {tareasCreadas} tareas correctamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error procesando el archivo",
                    Detail = ex.Message
                });
            }
        }

        private List<string> ValidarTareas(List<TareaImportada> tareas)
        {
            var errores = new List<string>();

            for (int i = 0; i < tareas.Count; i++)
            {
                var resultado = new List<ValidationResult>();
                var contexto = new ValidationContext(tareas[i]);

                if (!Validator.TryValidateObject(tareas[i], contexto, resultado, true))
                {
                    errores.Add($"Línea {i + 1}: {string.Join(", ", resultado.Select(r => r.ErrorMessage))}");
                }
            }

            return errores;
        }
    }
}