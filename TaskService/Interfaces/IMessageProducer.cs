namespace TaskService.Interfaces
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message);
    }
}