using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UserPortal.Dtos;
using UserPortal.Services.AsyncDataServices.Consts;
using UserPortal.Services.AsyncDataServices.Interfaces;

namespace UserPortal.Services.AsyncDataServices.Services
{
    public class MessageBusClient : BackgroundService, IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(
            IConfiguration configuration,
            IEventProcessor eventProcessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: RabbitMqConst.userCreateExchange, type: ExchangeType.Direct);

                _channel.QueueDeclare(RabbitMqConst.createUser);
                _channel.QueueBind(RabbitMqConst.createUser, RabbitMqConst.userCreateExchange, RabbitMqConst.createUser);

                _channel.QueueDeclare(RabbitMqConst.userActivision);
                _channel.QueueBind(RabbitMqConst.userActivision, RabbitMqConst.userCreateExchange, RabbitMqConst.userActivision);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                System.Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"--> Could not connect to the Message Bus {ex.Message}");
            }
        }

        public void PublishNewUser(UserCreatedDto userCreatedDto)
        {
            var message = JsonSerializer.Serialize(userCreatedDto);

            if (_connection.IsOpen)
            {
                System.Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                SendMessage(message);
            }
            else
            {
                System.Console.WriteLine("--> RabbitMQ Connection is closed, not sending...");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, eventArgs) =>
            {
                System.Console.WriteLine("--> Event Received!");

                var body = eventArgs.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMessage);
            };

            _channel.BasicConsume(queue: RabbitMqConst.userActivision, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: RabbitMqConst.userCreateExchange,
                                    routingKey: RabbitMqConst.createUser,
                                    basicProperties: null,
                                    body: body);

            System.Console.WriteLine($"--> We have sent {message}");
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("--> RabbitMQ Connection Shutdown.");
        }

        public void Dispose()
        {
            System.Console.WriteLine("--> Message Bus disposed");

            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
    }
}
