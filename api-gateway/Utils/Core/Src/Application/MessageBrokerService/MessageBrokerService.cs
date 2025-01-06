using RabbitMQ.Contracts;

namespace Application.Core 
{ 
    public interface IMessageBrokerService
    {
        Task Publish(CreateUser @event);
    }
}
