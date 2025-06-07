using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using TaskService.Controllers;
using TaskService.Data;
using TaskService.Models;
using TaskService.Interfaces;
using TaskService.DTOs;
using Microsoft.AspNetCore.Http;

namespace TaskService.Test
{
    public class TareasControllerUnitTests
    {
        private readonly Mock<ITareaRepository> _mockRepo;
        private readonly Mock<IMessageProducer> _mockMessageProducer;
        private readonly Mock<IFtpService> _mockFtpService;
        private readonly Mock<IUsuarioValidator> _mockUsuarioValidator;
        private readonly TareasController _controller;

        public TareasControllerUnitTests()
        {
            _mockRepo = new Mock<ITareaRepository>();
            _mockMessageProducer = new Mock<IMessageProducer>();
            _mockFtpService = new Mock<IFtpService>();
            _mockUsuarioValidator = new Mock<IUsuarioValidator>();

            _controller = new TareasController(
                _mockRepo.Object,
                _mockMessageProducer.Object,
                _mockFtpService.Object,
                _mockUsuarioValidator.Object
            )
            {                
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        
        [Fact]
        public async Task GetById_TareaExistente_RetornaOkConTarea()
        {
            // Arrange
            var testId = 1;
            var mockTarea = new Tarea { Id = testId, Titulo = "Tarea Test", CodigoTarea = "T001", Estado = true };
            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(testId)).ReturnsAsync(mockTarea);

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Tarea>(actionResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task GetById_TareaNoExistente_RetornaNotFound()
        {
            // Arrange
            var testId = 1;
            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(testId)).ReturnsAsync((Tarea)null);

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        
        [Fact]
        public async Task Create_ModeloValido_RetornaCreatedAtActionConTarea()
        {
            // Arrange
            var nuevaTarea = new Tarea { Titulo = "Nueva Tarea", Descripcion = "Desc", CodigoTarea = "T002", CriteriosAceptacion = "Criterios" };
            _mockRepo.Setup(repo => repo.CreateTareaAsync(It.IsAny<Tarea>()))
                .Callback<Tarea>(t => t.Id = 1)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(nuevaTarea);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Tarea>(actionResult.Value);
            Assert.Equal("Nueva Tarea", returnValue.Titulo);
            Assert.Equal(1, returnValue.Id);
            Assert.Equal(nameof(TareasController.GetById), actionResult.ActionName);
        }

        [Fact]
        public async Task Create_ModeloInvalido_RetornaBadRequest()
        {
            // Arrange
            var nuevaTarea = new Tarea { Titulo = "Tarea Sin Codigo" };
            _controller.ModelState.AddModelError("CodigoTarea", "El código de la tarea es obligatorio.");

            // Act
            var result = await _controller.Create(nuevaTarea);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.True(badRequestResult.Value is SerializableError);
        }

        
        [Fact]
        public async Task Update_TareaNoExistente_RetornaNotFound()
        {
            // Arrange
            var testId = 99;
            var tareaUpdateDto = new TareaUpdateDto { Titulo = "Actualizada", CodigoTarea = "T003", Descripcion = "D", CriteriosAceptacion = "CA", EstadoTarea = "Backlog" };
            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(testId)).ReturnsAsync((Tarea)null);

            // Act
            var result = await _controller.Update(testId, tareaUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"No se encontró la tarea con ID {testId}.", notFoundResult.Value);
        }

        [Fact]
        public async Task Update_EstadoTareaInvalidoEnDto_RetornaBadRequest()
        {
            // Arrange
            var testId = 1;
            var tareaUpdateDto = new TareaUpdateDto { Titulo = "Actualizada", CodigoTarea = "T003", Descripcion = "D", CriteriosAceptacion = "CA", EstadoTarea = "EstadoInvalido" };            

            // Act
            var result = await _controller.Update(testId, tareaUpdateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic value = badRequestResult.Value;
            
            Assert.Equal("El estado no es válido. Use: Backlog, Doing, InReview, Done.", value.GetType().GetProperty("estadoTarea").GetValue(value, null));
        }

        [Fact]
        public async Task Update_ModeloValido_RetornaNoContent()
        {
            // Arrange
            var testId = 1;
            var tareaExistente = new Tarea { Id = testId, Titulo = "Original", CodigoTarea = "T001", EstadoTarea = EstadoTarea.Backlog, Estado = true };
            var tareaUpdateDto = new TareaUpdateDto { Titulo = "Actualizada", CodigoTarea = "T001-Mod", Descripcion = "Desc Mod", CriteriosAceptacion = "CA Mod", EstadoTarea = "Doing" };

            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(testId)).ReturnsAsync(tareaExistente);
            _mockRepo.Setup(repo => repo.UpdateTareaAsync(It.IsAny<Tarea>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(testId, tareaUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(repo => repo.UpdateTareaAsync(It.Is<Tarea>(t =>
                t.Id == testId &&
                t.Titulo == tareaUpdateDto.Titulo &&
                t.CodigoTarea == tareaUpdateDto.CodigoTarea &&
                t.Descripcion == tareaUpdateDto.Descripcion &&
                t.CriteriosAceptacion == tareaUpdateDto.CriteriosAceptacion            
            )), Times.Once);
        }

        
        [Fact]
        public async Task AsignarTarea_TareaNoExistente_RetornaNotFound()
        {
            // Arrange
            var tareaId = 99;
            var asignacionDto = new AsignacionTareaDto { UsuarioId = 1 };
            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync((Tarea)null);

            // Act
            var result = await _controller.AsignarTarea(tareaId, asignacionDto);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Tarea no encontrada", actionResult.Value);
        }

        [Fact]
        public async Task AsignarTarea_TareaFinalizada_RetornaBadRequest()
        {
            // Arrange
            var tareaId = 1;
            var tareaFinalizada = new Tarea { Id = tareaId, Titulo = "Finalizada", EstadoTarea = EstadoTarea.Done, Estado = true };
            var asignacionDto = new AsignacionTareaDto { UsuarioId = 1 };
            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync(tareaFinalizada);

            // Act
            var result = await _controller.AsignarTarea(tareaId, asignacionDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No se puede asignar una tarea finalizada", actionResult.Value);
        }

        [Fact]
        public async Task AsignarTarea_UsuarioNoExiste_RetornaNotFound()
        {
            // Arrange
            var tareaId = 1;
            var tareaAsignable = new Tarea { Id = tareaId, Titulo = "Asignable", EstadoTarea = EstadoTarea.Backlog, Estado = true };
            var asignacionDto = new AsignacionTareaDto { UsuarioId = 99 };

            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync(tareaAsignable);
            _mockUsuarioValidator.Setup(validator => validator.UsuarioExisteAsync(asignacionDto.UsuarioId)).ReturnsAsync(false);

            // Act
            var result = await _controller.AsignarTarea(tareaId, asignacionDto);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Usuario no existe", actionResult.Value);
        }

        [Fact]
        public async Task AsignarTarea_Exitoso_RetornaNoContent_Y_EnviaMensaje()
        {
            // Arrange
            var tareaId = 1;
            var usuarioId = 5;
            var tareaAsignable = new Tarea { Id = tareaId, Titulo = "Tarea para Asignar", EstadoTarea = EstadoTarea.Backlog, Estado = true };
            var asignacionDto = new AsignacionTareaDto { UsuarioId = usuarioId };

            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync(tareaAsignable);
            _mockUsuarioValidator.Setup(validator => validator.UsuarioExisteAsync(usuarioId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.UpdateTareaAsync(It.IsAny<Tarea>())).Returns(Task.CompletedTask);
            _mockMessageProducer.Setup(mp => mp.SendMessage(It.IsAny<AsignacionTareaMessage>()));

            // Act
            var result = await _controller.AsignarTarea(tareaId, asignacionDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(repo => repo.UpdateTareaAsync(It.Is<Tarea>(t => t.Id == tareaId && t.UsuarioId == usuarioId)), Times.Once);
            _mockMessageProducer.Verify(mp => mp.SendMessage(It.Is<AsignacionTareaMessage>(m =>
                m.TareaId == tareaId &&
                m.UsuarioId == usuarioId &&
                m.TituloTarea == tareaAsignable.Titulo)), Times.Once);
        }

        [Fact]
        public async Task Patch_ActualizarEstado_Exitoso_RetornaNoContent()
        {
            // Arrange
            var tareaId = 1;
            var tiempoDesarrolloValido = 5;
            var tareaOriginal = new Tarea { Id = tareaId, Titulo = "Tarea Original", EstadoTarea = EstadoTarea.Doing, UsuarioId = 1, 
                FechaInicio = System.DateTime.UtcNow.AddDays(-1), Estado = true, Descripcion = "Descripcion", CodigoTarea = "MOCK-TASK", CriteriosAceptacion = "Criterios" };
                        
            var patchDoc = new JsonPatchDocument<Tarea>();
            patchDoc.Replace(t => t.EstadoTarea, EstadoTarea.InReview);
            patchDoc.Replace(t => t.TiempoDesarrollo, tiempoDesarrolloValido);


            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync(tareaOriginal);
            _mockRepo.Setup(repo => repo.UpdateTareaAsync(It.IsAny<Tarea>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Patch(tareaId, patchDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(repo => repo.UpdateTareaAsync(It.Is<Tarea>(t =>
                t.Id == tareaId &&
                t.EstadoTarea == EstadoTarea.InReview &&
                t.TiempoDesarrollo == tiempoDesarrolloValido
            )), Times.Once);
        }

        [Fact]
        public async Task Patch_CampoNoPermitido_RetornaBadRequest()
        {
            // Arrange
            var tareaId = 1;
            var tareaOriginal = new Tarea { Id = tareaId, Titulo = "Tarea Original", EstadoTarea = EstadoTarea.Backlog, UsuarioId = 0, Estado = true };
            var patchDoc = new JsonPatchDocument<Tarea>();
            patchDoc.Replace(t => t.Titulo, "Nuevo Titulo No Permitido");

            _mockRepo.Setup(repo => repo.GetTareaByIdAsync(tareaId)).ReturnsAsync(tareaOriginal);

            // Act
            var result = await _controller.Patch(tareaId, patchDoc);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic value = badRequestResult.Value;
            Assert.Equal("No se permite modificar el campo: /Titulo", value.GetType().GetProperty("error").GetValue(value, null));
        }
    }
}