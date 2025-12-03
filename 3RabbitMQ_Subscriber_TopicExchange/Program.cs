using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: false);


        var consumer = new AsyncEventingBasicConsumer(channel);

        var result = await channel.QueueDeclareAsync();
        var qname = result.QueueName;

        var rootKey = "*.Error.*";
        await channel.QueueBindAsync(qname, "logs-topic", "*.Error.*");//kuyruk BİND etmek abone duştuğunde kuyruk ta kapanır 

        await channel.BasicConsumeAsync(          // await → işlemi asenkron yürütür
            qname,                                // string qname → dinlenecek kuyruk adı
            false,                                // bool autoAck → otomatik onay kapalı
            consumer                              // AsyncEventingBasicConsumer consumer → mesajları işleyecek tüketici
        );
        Console.WriteLine("loglar dinleniyor");
        consumer.ReceivedAsync += (sender, e) =>        // object sender, BasicDeliverEventArgs e → mesaj geldiğinde tetiklenen event
        {
            var message = Encoding.UTF8.GetString(       // string message → gelen mesaj metni
                e.Body.ToArray()                         // byte[] → mesajın byte dizisine çevrilmesi
            );
            Console.WriteLine("Gelen mesaj: " + message); // Konsola mesajı yazdırı
            File.AppendAllText(                          // Mesajı dosyaya ekler
                "log-critical.txt",                      // string → yazılacak dosya adı
                message + "\n"                           // string → yazılacak içerik
            );
            channel.BasicAckAsync(                       // Mesajı başarıyla işlendi olarak işaretler
                e.DeliveryTag,                           // ulong DeliveryTag → mesajın benzersiz etiketi
                false                                    // bool multiple → tek bir mesajı onayla
            );
            return Task.CompletedTask;
            Console.ReadLine();
        };
    }
}