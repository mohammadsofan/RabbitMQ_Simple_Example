using RabbitMQ.Client;
using System.Text;
namespace MessageSender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "MessageSenderClient";
            IConnection connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            var exchange = "test_exchange";
            var queueName = "test_queue";
            var routekey = "test_route";
            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, false,false,null);
            await channel.QueueDeclareAsync(queueName, false, false, false, null);
            await channel.QueueBindAsync(queueName, exchange, routekey, null);
            for (int i = 0; i < 10; i++)
            {
                Task.Delay(1000).Wait(); 
                var message = $"Message {i + 1}";
                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange, routekey, body);
                Console.WriteLine($"Message sent: {message}");
            }
            await channel.CloseAsync();
            await connection.CloseAsync();
            Console.WriteLine("Connection closed.");

        }
    }
}
