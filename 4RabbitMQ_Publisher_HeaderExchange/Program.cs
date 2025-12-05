using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

internal class Program
{
    public enum LogsName // Log seviyelerini enum olarak yazdım, daha düzenli olsun diye
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }

    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory(); // RabbitMQ bağlantısı kurmak için fabrika nesnesi oluşturuyorum
        factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr"); // CloudAMQP bağlantı adresini verdim

        using var connection = await factory.CreateConnectionAsync(); // Sunucuya async şekilde bağlantı açıyorum
        using var channel = await connection.CreateChannelAsync(); // Açılan bağlantı üzerinden bir channel oluşturuyorum

        await channel.ExchangeDeclareAsync(
            "header-exchange",               // Exchange adını yazıyorum
            type: ExchangeType.Headers,      // Tipi header exchange olarak belirttim
            durable: true                    // Sunucu kapanınca kaybolmasın diye durable yaptım
        );

        Dictionary<string, object> headers = new Dictionary<string, object>(); // Header bilgilerini tutacak dictionary oluşturdum
        headers.Add("format", "pdf"); // Header’a format bilgisini ekledim
        headers.Add("shape", "a4");   // Header’a sayfa şekli bilgisini ekledim

        var properties = new BasicProperties // Mesajın özelliklerini burada oluşturuyorum
        {
            Headers = headers // Oluşturduğum header sözlüğünü buraya veriyorum
        };
        properties.Persistent = true;

        var product = new Product//class lıb içerisinden product classı ıverisnden nesne oluşturdum ve proplara değer verdik 
        {
            Id = 1,
            Name = "Laptop",
            Price = 15000,
            Stock = 5

        };

        var productJsonSting= JsonSerializer.Serialize(product);//oluşturduğum product nesnesini json stringe çevirdim
        //mesajları serialize etme işlemi yaptık /VERİLERİN OKUNABILIR FOTMATTA GONDERİLMESİ İÇİN //DE-SERIALIZE İŞLEMİ SE VERİLERİ OKNUMAFORMATINA ÇEVİRMEK 



        string message = "This is a header exchange test message."; // Göndermek istediğim mesaj
        var body = Encoding.UTF8.GetBytes(productJsonSting); // Mesajı byte dizisine çevirdim çünkü RabbitMQ byte alıyor

        await channel.BasicPublishAsync(
            exchange: "header-exchange",  // Mesajı göndereceğim exchange
            routingKey: string.Empty,     // Header exchange routing key kullanmadığı için boş geçiyorum
            mandatory: false,             // Mesaj zorunlu değil, kuyruk yoksa geri dönmesin diye false yaptım
            basicProperties: properties,  // Mesajın header ve diğer özellikleri burada
            body: body                    // Göndereceğim asıl mesaj (byte formatında)
        );

        Console.WriteLine("mesaj gönderildi."); // Mesajın gittiğini konsola yazdırıyorum

        Console.ReadLine(); // Console kapanmasın diye bekletiyorum
    }


}

