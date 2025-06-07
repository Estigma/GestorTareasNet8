using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text.Json;
using TaskService.DTOs;
using TaskService.Interfaces;

namespace TaskService.Services
{
    public class UsuarioValidatorRabbitMQ : IUsuarioValidator, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<UsuarioValidationResponse>> _pendingRequests = new();

        public UsuarioValidatorRabbitMQ(IConnectionFactory factory)
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare().QueueName;

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var response = JsonSerializer.Deserialize<UsuarioValidationResponse>(ea.Body.ToArray());

                    if (_pendingRequests.TryRemove(response.RequestId, out var tcs))
                        tcs.SetResult(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en consumer: {ex.Message}");
                }

                await Task.CompletedTask;
            };

            _channel.BasicConsume(_replyQueueName, true, consumer);
        }


        public async Task<bool> UsuarioExisteAsync(int usuarioId)
        {
            var request = new UsuarioValidationRequest { UsuarioId = usuarioId };
            var tcs = new TaskCompletionSource<UsuarioValidationResponse>();
            _pendingRequests[request.RequestId] = tcs;

            var props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = request.RequestId.ToString();

            _channel.BasicPublish(
                exchange: "",
                routingKey: "usuario-validation-queue",
                basicProperties: props,
                body: JsonSerializer.SerializeToUtf8Bytes(request));

            var response = await tcs.Task;
            return response.Existe;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
