using Microsoft.EntityFrameworkCore;
using TaskService.DTOs;
using TaskService.Models;

namespace TaskService.Data
{
    public class TareaRepository : ITareaRepository
    {
        private readonly AppDbContext _context;

        public TareaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tarea>> GetAllTareasAsync()
        {
            return await _context.Tareas.Where(t => t.Estado).ToListAsync();
        }

        public async Task<Tarea> GetTareaByIdAsync(int id)
        {
            return await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.Estado);
        }

        public async Task CreateTareaAsync(Tarea tarea)
        {
            await _context.Tareas.AddAsync(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTareaAsync(Tarea tarea)
        {
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTareaAsync(int id)
        {
            var tarea = await GetTareaByIdAsync(id);
            if (tarea != null)
            {
                tarea.Estado = false;
                await UpdateTareaAsync(tarea);
            }
        }

        public async Task<IEnumerable<Tarea>> GetTareasByUsuarioAsync(int usuarioId)
        {
            return await _context.Tareas
                .Where(t => t.UsuarioId == usuarioId && t.Estado)
                .ToListAsync();
        }

        public async Task<int> ImportarTareasAsync(List<TareaImportada> tareasImportadas)
        {
            var tareasCreadas = 0;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var tareaImportada in tareasImportadas)
                {
                    var existe = await _context.Tareas.AnyAsync(t => t.CodigoTarea == tareaImportada.CodigoTarea);

                    if (!existe)
                    {
                        var nuevaTarea = new Tarea
                        {
                            CodigoTarea = tareaImportada.CodigoTarea,
                            Titulo = tareaImportada.Titulo,
                            Descripcion = tareaImportada.Descripcion,
                            CriteriosAceptacion = tareaImportada.CriteriosAceptacion,                                                        
                            EstadoTarea = EstadoTarea.Backlog,
                            Estado = true
                        };

                        await _context.Tareas.AddAsync(nuevaTarea);
                        tareasCreadas++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return tareasCreadas;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}