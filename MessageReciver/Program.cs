using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace MessageReciver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672"),
                ClientProvidedName = "MessageReciverClient"
            };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            var exchangeName = "test_exchange";
            var queueName = "test_queue";
            var routeKey = "test_route";
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, false, false, null);
            await channel.QueueDeclareAsync(queueName, false, false, false, null);
            await channel.QueueBindAsync(queueName, exchangeName, routeKey, null);
            //what Qos means: Quality of Service (QoS) settings control how many messages the consumer can process at once.
            await channel.BasicQosAsync(0, 1, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync+= async(sender,content)=>
            {
                Task.Delay(3000).Wait();
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message received: {message}");
                await channel.BasicAckAsync(content.DeliveryTag, false);
            };
            string consumerTag = await channel.BasicConsumeAsync(queueName, false, consumer);
            Console.ReadLine();
            await channel.BasicCancelAsync(consumerTag);
            await channel.CloseAsync();
            await connection.CloseAsync();
        }
    }
}
