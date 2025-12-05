using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

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


        await channel.ExchangeDeclareAsync(
            "header-exchange",               // Exchange adını yazıyorum
            type: ExchangeType.Headers,      // Tipi header exchange olarak belirttim
            durable: true                    // Sunucu kapanınca kaybolmasın diye durable yaptım
        );

        Dictionary<string, object> headers = new Dictionary<string, object>(); // Header bilgilerini tutacak dictionary oluşturdum
        headers.Add("format", "pdf"); // Header’a format bilgisini ekledim
        headers.Add("shape", "a4");   // Header’a sayfa şekli bilgisini ekledim
        headers.Add("x-match","all");

        var properties = new BasicProperties // Mesajın özelliklerini burada oluşturuyorum
        {
            Headers = headers // Oluşturduğum header sözlüğünü buraya veriyorum
        };

        properties.Persistent = true; // mesajın kalıcı olması için

        await channel.QueueBindAsync(qname, "header-exchange",string.Empty,headers);//en sonda HEADER EXCHANGE için kuyruk bağlama işlemi yapılır
        //kuyruk BİND etmek abone duştuğunde kuyruk ta kapanır 

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

            Product product= JsonSerializer.Deserialize<Product>(message);

            Console.WriteLine("Gelen mesaj: " + product.Id,product.Name,product.Price,product.Stock); // Konsola mesajı yazdırı
            File.AppendAllText(                          // Mesajı dosyaya ekler
                "log-critical.txt",                      // string → yazılacak dosya adı
                product + "\n"                           // string → yazılacak içerik
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
