using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace AddressBook.HelperService
{
    public class RabbitMQService
    {
        private readonly IConfiguration _configuration;
        private readonly string _queueName;

        public RabbitMQService(IConfiguration configuration)
        {
            _configuration = configuration;
            _queueName = _configuration["RabbitMQ:QueueName"];
        }

        public async Task PublishMessageAsync(object message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties();
            properties.Persistent = true;

            await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, basicProperties: properties, body: body);
        }
    
}
}
