using RabbitMQComponent.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IChannelConnection channelConnection;

        public ConnectionService(IChannelConnection _channelConnection)
        {
            channelConnection = _channelConnection;
        }

        public Task<int> GetInfo()
        {
            return Task.FromResult(channelConnection.GetMeesageSize().Result);
        }

        public Task Start()
        {
            Task.Run(() =>
            {
                var list = new List<Task>();
                list.Add(channelConnection.CreateConsumerConnection());
                list.Add(channelConnection.CreateConnection());

                return Task.FromResult(Task.WhenAll(list));
            });

            return Task.CompletedTask;
        }
    }
}
