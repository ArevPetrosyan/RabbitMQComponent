namespace RabbitMQComponent.Models.Config
{
    public abstract class BaseConfigModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public string VHost { get; set; }
        public string Exchange { get; set; }
        public int Port { get; set; }
    }
}
