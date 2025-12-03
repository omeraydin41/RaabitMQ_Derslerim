using RabbitMQ.Client;
using System.Text;

internal class Program
{
    public enum LogsName
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }

    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            "logs-topic",
            type: ExchangeType.Topic,//exchange tipini değiştirdik 
            durable: true
        );



        var list = Enumerable.Range(1, 20).ToList();

        foreach (var item in list)
        {

            Random rnd = new Random();
            LogsName log1 = (LogsName)rnd.Next(1, 5);
            LogsName log2 = (LogsName)rnd.Next(1, 5);
            LogsName log3 = (LogsName)rnd.Next(1, 5);

            var rotKey = $"{log1}.{log2}.{log3}";

            var message = $"{log1}-{log2}-{log3} log!";
            var bodyMessage = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "logs-topic",
                routingKey: rotKey,
                mandatory: false,
                body: bodyMessage
            );

            Console.WriteLine($"log gönderildi : {message}");

        }

        Console.ReadLine();
    }
}
