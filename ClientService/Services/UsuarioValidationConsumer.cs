using ClientService.Data;
using ClientService.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using IModel = RabbitMQ.Client.IModel;

namespace ClientService.Services
{
    public class UsuarioValidationConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public UsuarioValidationConsumer(
            IConnection connection,
            IServiceProvider serviceProvider)
        {
            _channel = connection.CreateModel();
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.QueueDeclare("usuario-validation-queue", durable: true);
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

                var request = JsonSerializer.Deserialize<UsuarioValidationRequest>(ea.Body.ToArray());
                var response = new UsuarioValidationResponse { RequestId = request.RequestId };

                try
                {
                    response.Existe = await repository.VerificarUsuarioExisteAsync(request.UsuarioId);
                }
                catch (Exception ex)
                {
                    response.Error = ex.Message;
                }

                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: ea.BasicProperties.ReplyTo,
                    basicProperties: replyProps,
                    body: JsonSerializer.SerializeToUtf8Bytes(response));
            };

            _channel.BasicConsume("usuario-validation-queue", true, consumer);
        }
    }
}

