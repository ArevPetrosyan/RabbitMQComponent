using RabbitMQ.Client;
using RabbitMQComponent.Interfaces;
using RabbitMQComponent.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQComponent.Connection
{
    public class ConsumerConnection : BaseConnection, IConsumerConnection
    {
        private readonly IConfigDataService appSettings;
        private readonly Timer timer;
        private const int intervalTime = 5;

        public ConsumerConnection(IConfigDataService _appSettings)
        {
            timer = new Timer(Restart);
            appSettings = _appSettings;
            ConnectionFactory = new ConnectionFactory()
            {
                UserName = appSettings.TargetProxyModel.Username,
                Password = appSettings.TargetProxyModel.Password,
                HostName = appSettings.TargetProxyModel.HostName,
                VirtualHost = appSettings.TargetProxyModel.VHost,
                Port = appSettings.TargetProxyModel.Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(intervalTime)
            };
        }

        public new Task CreateConnection()
        {
            try
            {
                Console.WriteLine($"ConsumerConnection-->CreateConnection-->Try to connect...");
                base.CreateConnection();

                Model.ExchangeDeclare(appSettings.TargetProxyModel.Exchange, ExchangeType.Fanout);
                Properties = Model.CreateBasicProperties();
                Properties.Persistent = false;

            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
            {
                // apply retry logic
                Console.WriteLine($"ConsumerConnection-->Exception BrokerUnreachableException: {e.Message}");
                Thread.Sleep(5000);
                timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(intervalTime));
            }

            return Task.CompletedTask;
        }

        private void Restart(object value)
        {
            try
            {
                if (base.IsOpen())
                {
                    Console.WriteLine("ConsumerConnection IsOpen - true, Time Stopped!");
                    timer?.Change(Timeout.Infinite, 0);
                }
                else
                {
                    Console.WriteLine("ConsumerConnection-->Restart-->CreateConnection");
                    base.CreateConnection();
                    Model.ExchangeDeclare(appSettings.TargetProxyModel.Exchange, ExchangeType.Fanout);
                    Properties = Model.CreateBasicProperties();
                    Properties.Persistent = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConsumerConnection-->Restart Exception: {ex.Message}");
            }
        }

        public Task PublishMessge(byte[] msg)
        {
            if (Model != null && Model.IsOpen)
            {
                Model.BasicPublish(appSettings.TargetProxyModel.Exchange, "", Properties, msg);
            }
            return Task.CompletedTask;
        }
    }
}
