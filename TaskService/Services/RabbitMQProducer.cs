using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using TaskService.Interfaces;

namespace TaskService.Services
{
    public class RabbitMQProducer : IMessageProducer
    {        
        private readonly IModel _channel;
        private const string ExchangeName = "asignaciones_exchange";

        public RabbitMQProducer(IConnectionFactory _connectionFactory)
        {
            var _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);
        }

        public void SendMessage<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: string.Empty,
                basicProperties: null,
                body: body);
            
            Console.WriteLine($" [x] Sent {json}");
        }
    }
}