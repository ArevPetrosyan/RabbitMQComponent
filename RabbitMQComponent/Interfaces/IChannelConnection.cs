using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Interfaces
{
    public interface IChannelConnection
    {
        Task CreateConsumerConnection();
        Task CreateConnection();
        Task<int> GetMeesageSize();
    }
}
