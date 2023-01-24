using RabbitMQComponent.Interfaces;

namespace RabbitMQComponent.Models.Config
{
    public class FeedConstructModel : BaseConfigModel
    {
        public string QueueName { get; set; }
        public string? PartnerId { get; set; }
    }
}
