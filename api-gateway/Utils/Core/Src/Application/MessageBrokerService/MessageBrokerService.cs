using RabbitMQ.Contracts;

namespace Application.Core 
{ 
    public interface IMessageBrokerService
    {
        Task Publish(CreateUser @event);
        Task Publish(CreateTowDriver @event);
    }
}
