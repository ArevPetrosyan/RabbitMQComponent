using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Interfaces
{
    public interface IConnectionService
    {
        Task Start();
        Task<int> GetInfo();
    }
}
