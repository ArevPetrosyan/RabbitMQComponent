using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Interfaces
{
    public interface IConsumerConnection
    {
        Task CreateConnection();
        Task PublishMessge(byte[] msg);
    }
}
