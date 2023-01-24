using RabbitMQComponent.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using ICSharpCode.SharpZipLib.GZip;

namespace RabbitMQComponent.Services
{
    public class ChannelConnection : BaseConnection, IChannelConnection
    {
        private EventingBasicConsumer consumer { get; set; }
        private readonly IConfigDataService appSettings;
        private readonly IConsumerConnection consumerConnection;
        private const int intervalTime = 5;
        private readonly Timer timer;

        public ChannelConnection(IConfigDataService _appSettings, IConsumerConnection _consumerConnection)
        {
            timer = new Timer(Restart);
            appSettings = _appSettings;
            consumerConnection = _consumerConnection;
            ConnectionFactory = new ConnectionFactory()
            {
                Uri = new Uri($"amqp://{appSettings.FeedConstructModel.Username}:{appSettings.FeedConstructModel.Password}@{appSettings.FeedConstructModel.HostName}:{appSettings.FeedConstructModel.Port}/{appSettings.FeedConstructModel.VHost}"),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(intervalTime)
            };
        }

        public async Task CreateConsumerConnection()
        {
            Console.WriteLine("Create Consumer Connection");
            await consumerConnection.CreateConnection();
        }

        public async Task CreateConnection()
        {
            try
            {
                if (base.IsOpen())
                {
                    Console.WriteLine("IsOpen - true, Time Stopped!");
                    timer?.Change(Timeout.Infinite, 0);
                }
                else
                {
                    Console.WriteLine("ChannelConnection-->CreateConnection-->Try to connect...");
                    base.CreateConnection();
                    if (appSettings.FeedConstructModel.PartnerId is null)
                    {
                        var queue = Model.QueueDeclare();
                        Model.QueueBind(queue.QueueName, appSettings.FeedConstructModel.Exchange, "");
                    }
                    CreateEventingBasicConsumer();
                    SetBasicConsume();
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
            {
                // apply retry logic
                Console.WriteLine($"ChannelConnection-->CreateConnection-->Exception-->{e.Message}");
                Console.WriteLine($"Sleep 5 sec and start");
                Thread.Sleep(5000);
                timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(intervalTime));
            }
        }

        public void SetBasicConsume()
        {
            if (appSettings.FeedConstructModel.PartnerId is null)
            {
                Model.BasicConsume(appSettings.FeedConstructModel.QueueName, true, consumer);
            }
            //else
            //{
            //    Model.BasicConsume($"P{appSettings.FeedConstructModel.PartnerId}_live", true, consumer2);
            //    Model.BasicConsume($"P{appSettings.FeedConstructModel.PartnerId}_prematch", true, consumer2);
            //}
        }

        public void CreateEventingBasicConsumer()
        {
            consumer = new EventingBasicConsumer(Model);
            consumer.Received += Consumer_Received;
            consumer.ConsumerCancelled += ConsumerCancelled;
            consumer.Unregistered += Unregistered;
            consumer.Shutdown += Shutdown;
        }

        private void Shutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("ChannelConnection-->Event Shutdown-->Sleep 5 sec and start");
            Thread.Sleep(5000);
            timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(intervalTime));
        }

        private void Restart(object value)
        {
            try
            {
                Console.WriteLine("ChannelConnection-->Restart-->CreateConnection");
                base.CreateConnection();
                if (appSettings.FeedConstructModel.PartnerId is null)
                {
                    var queue = Model.QueueDeclare();
                    Model.QueueBind(queue.QueueName, appSettings.FeedConstructModel.Exchange, "");
                }
                CreateEventingBasicConsumer();
                SetBasicConsume();
                if (base.IsOpen())
                {
                    Console.WriteLine("ChannelConnection IsOpen - true, Time Stopped!");
                    timer?.Change(Timeout.Infinite, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChannelConnection-->Exception-->{ex.Message}");
            }
        }

        private void Unregistered(object? sender, ConsumerEventArgs e)
        {

        }

        private void ConsumerCancelled(object? sender, ConsumerEventArgs e)
        {

        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            LastMeesageSize = @event.Body.Length;
            consumerConnection.PublishMessge(@event.Body.ToArray());
        }

        public Task<int> GetMeesageSize()
        {
            return Task.FromResult(LastMeesageSize);
        }

        private byte[] DecompressBytes(byte[] data)
        {
            using var inStream = new MemoryStream(data);
            using var outStream = new MemoryStream();
            using var gzStream = new GZipStream(inStream, CompressionMode.Decompress);
            gzStream.CopyTo(outStream);
            return outStream.ToArray();
        }

        private async Task<byte[]> DecompressBytesAsync(byte[] bytes, CancellationToken cancel = default)
        {
            await using var inStream = new MemoryStream(bytes);
            await using var outStream = new MemoryStream();
            await using (var gzStream = new GZipInputStream(inStream))
                await gzStream.CopyToAsync(outStream, CancellationToken.None);

            return outStream.ToArray();
        }
    }
}
