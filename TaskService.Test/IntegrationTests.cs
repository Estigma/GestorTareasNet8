using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using TaskService.Data;
using TaskService.DTOs;
using TaskService.Models;
using Testcontainers.RabbitMq;
using IModel = RabbitMQ.Client.IModel;

namespace TaskService.Test
{
    public class IntegrationTests : IAsyncLifetime
    {
        private readonly RabbitMqContainer _rabbitContainer;
        private IConnection _connection;
        private IModel _channel;
        private string _messageReceived = null;

        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests()
        {
            _rabbitContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3-management")                
                .WithPortBinding(5672, true)
                .WithEnvironment("RABBITMQ_DEFAULT_USER", "test")
                .WithEnvironment("RABBITMQ_DEFAULT_PASS", "test")
                .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5672))

                .Build();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TaskService.Data.AppDbContext));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        services.AddDbContext<AppDbContext>(options =>
                            options.UseInMemoryDatabase("TestDb"));

                        descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(RabbitMQ.Client.IConnectionFactory));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        services.AddSingleton<IConnectionFactory>(sp =>
                        {
                            return new ConnectionFactory()
                            {
                                HostName = _rabbitContainer.Hostname,
                                Port = _rabbitContainer.GetMappedPublicPort(5672),
                                UserName = "test",
                                Password = "test",
                                DispatchConsumersAsync = true,
                                AutomaticRecoveryEnabled = true,
                                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                            };
                        });
                    });
                });
            
        }

        public async Task InitializeAsync()
        {

            var dockerClient = new DockerClientConfiguration().CreateClient();
            var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true });

            await _rabbitContainer.StartAsync();

            var factory = new ConnectionFactory
            {
                HostName = _rabbitContainer.Hostname,
                Port = _rabbitContainer.GetMappedPublicPort(5672),
                UserName = "test",
                Password = "test"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "asignaciones_exchange", type: ExchangeType.Fanout);
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName, exchange: "asignaciones_exchange", routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                _messageReceived = Encoding.UTF8.GetString(ea.Body.ToArray());
                await Task.CompletedTask;
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            var client = _factory.CreateClient();

            var nuevaTarea = new
            {
                codigoTarea = "TASK-001",
                titulo = "Prueba 1",
                descripcion = "Prueba 1",
                criteriosAceptacion = "Aceptar",
                tiempoDesarrollo = 1,
                estadoTarea = "Backlog",
                estado = true,
                usuarioId = 1
            };

            await client.PostAsJsonAsync("/api/tareas", nuevaTarea);

            nuevaTarea = new
            {
                codigoTarea = "TASK-002",
                titulo = "Prueba 2",
                descripcion = "Prueba 2",
                criteriosAceptacion = "Aceptar",
                tiempoDesarrollo = 1,
                estadoTarea = "Backlog",
                estado = true,
                usuarioId = 1
            };

            await client.PostAsJsonAsync("/api/tareas", nuevaTarea);
        }

        [Fact]
        public async Task CrearYLeerTarea_OK()
        {
            var client = _factory.CreateClient();

            var nuevaTarea = new
            {
                codigoTarea = "TASK-002",
                titulo = "Prueba",
                descripcion = "Prueba",
                criteriosAceptacion = "Aceptar",
                tiempoDesarrollo = 1,
                estadoTarea = "Backlog",
                estado = true,
                usuarioId = 1
            };

            var postResponse = await client.PostAsJsonAsync("/api/tareas", nuevaTarea);
            postResponse.EnsureSuccessStatusCode();

            var tareaCreada = await postResponse.Content.ReadFromJsonAsync<Tarea>();
            tareaCreada.Should().NotBeNull();

            var getResponse = await client.GetAsync($"/api/tareas/{tareaCreada.Id}");
            getResponse.EnsureSuccessStatusCode();

            var tareaObtenida = await getResponse.Content.ReadFromJsonAsync<Tarea>();
            tareaObtenida.Id.Should().Be(tareaCreada.Id);
        }

        [Fact]
        public async Task PostTarea_Error()
        {
            var client = _factory.CreateClient();

            var nuevaTarea = new
            {
                codigoTarea = "TASK-RMQ",                
                descripcion = "Test integrando colas",
                criteriosAceptacion = "Mensajes",
                tiempoDesarrollo = 3,
                estadoTarea = "Doing",
                estado = true,
                usuarioId = 1
            };

            var response = await client.PostAsJsonAsync("/api/tareas", nuevaTarea);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAll()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/tareas");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Error()
        {
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync("/api/tareas/5");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task PutTarea_Error()
        {
            var client = _factory.CreateClient();

            var nuevaTarea = new TareaUpdateDto
            {
                Id = 3,
                CodigoTarea = "TASK-RMQ",
                Descripcion = "Test integrando colas",
                CriteriosAceptacion = "Mensajes",
                TiempoDesarrollo = 3,
                EstadoTarea = "Doing",
                Estado = true,
                UsuarioId = 1
            };

            var httpContent = JsonContent.Create(nuevaTarea);

            var response = await client.PutAsync($"/api/tareas/{nuevaTarea.Id}", httpContent);

            var errorDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            errorDetails.Should().NotBeNull();
            errorDetails.Errors.Should().ContainKey("Titulo");
            errorDetails.Errors["Titulo"].First().Should().Be("El título es obligatorio.");
        }

        [Fact]
        public async Task PutTarea_OK()
        {
            var client = _factory.CreateClient();

            var nuevaTarea = new TareaUpdateDto
            {
                Id = 1,
                CodigoTarea = "TASK-RMQ",
                Titulo = "Test Integrando colas",
                Descripcion = "Test integrando colas",
                CriteriosAceptacion = "Mensajes",
                TiempoDesarrollo = 3,
                EstadoTarea = "Doing",
                Estado = true,
                UsuarioId = 1
            };

            var httpContent = JsonContent.Create(nuevaTarea);

            var response = await client.PutAsync($"/api/tareas/{nuevaTarea.Id}", httpContent);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await client.GetAsync($"/api/tareas/{nuevaTarea.Id}");
            var tarea = await getResponse.Content.ReadFromJsonAsync<Tarea>();
            tarea.Should().NotBeNull();
            tarea.CodigoTarea.Should().Be(nuevaTarea.CodigoTarea);
        }
        public async Task DisposeAsync()
        {
            _channel?.Close();
            _connection?.Close();
            await _rabbitContainer.DisposeAsync();
        }
    }
}
