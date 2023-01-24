using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Services
{
    public abstract class BaseConnection
    {
        public ConnectionFactory ConnectionFactory;
        public IModel Model;
        public int LastMeesageSize = 0;
        public IBasicProperties Properties;
        private IConnection connection;

        public void CreateConnection()
        {
            try
            {
                connection = ConnectionFactory.CreateConnection();
                Model = connection.CreateModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BaseConnection-->CreateConnection-->Exception-->{ex.Message}");
                throw new BrokerUnreachableException(ex);
            }
        }

        public bool IsOpen()
        {
            return connection == null ? false : connection.IsOpen;
        }
    }
}
