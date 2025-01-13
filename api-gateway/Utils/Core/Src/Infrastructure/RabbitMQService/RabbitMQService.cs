using MassTransit;
using RabbitMQ.Contracts;

namespace Application.Core
{
    public class RabbitMQService(IPublishEndpoint publishEndpoint) : IMessageBrokerService
    {
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        public async Task Publish(CreateUser @event)
        {
            await _publishEndpoint.Publish(@event); 
        }
    }
} 