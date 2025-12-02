using RabbitMQ.Client;           // RabbitMQ client kütüphanesi
using RabbitMQ.Client.Events;    // EventingBasicConsumer için gerekli
using System.Text;               // Byte -> string dönüşümü için


var factory = new ConnectionFactory();


factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");


using var connection = await factory.CreateConnectionAsync();


using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "ikinci.q", // Kuyruğumuzun adı 'birQ' olsun.
                             durable: false, // **ÖNEMLİ:** RabbitMQ sunucusu yeniden başlasa bile kuyruğun silinmemesini sağlar.
                             exclusive: false, // Bu kuyruğu sadece biz değil,channel den başka değişkenlerde erişsin.subscriber tarafından erişilecek 
                             autoDelete: false, // Son kullanıcı bağlantısı kapansa bile kuyruk otomatik silinmesin.
                             arguments: null); // Ekstra ayar yok.







await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

//prefetchSize: 0 → Mesaj boyutuna göre sınır yok. Yani RabbitMQ, mesaj boyutuna bakmadan gönderebilir.
//globak false olursa prefetchCount da yazan değer kadar her abonye aynı anda o kadar mesaj gönderir.mesela 1
//global true olursa örnek 2 abone 5 mesaj varsa toplam 3 2 şki olarak ayarlar yani 5 tane mesajı tum aboneler arasında dağıtır





// Kanaldan mesajları asenkron şekilde tüketmek için AsyncEventingBasicConsumer sınıfını kullanıyoruz.
// Bu consumer, gelen mesajları Event tabanlı bir mantıkla (ReceivedAsync) dinler.
var consumer = new AsyncEventingBasicConsumer(channel);

// "birQ" isminde kuyruğu tüketmeye (dinlemeye) başlatıyoruz.
// 2. parametre: autoAck (false = manuel onaylama) -> Mesajı biz onaylayacağız.
// consumer: Mesajları teslim edecek tüketici.
await channel.BasicConsumeAsync("ikinci.q", false, consumer);

// Mesaj her geldiğinde tetiklenecek event.
// sender: olayı tetikleyen consumer
// BasicDeliverEventArgs e: Mesajla ilgili tüm bilgileri içerir (Body, RoutingKey, Exchange, Redelivered vb.)
consumer.ReceivedAsync += (object sender, BasicDeliverEventArgs e) =>
{
    // e.Body -> mesajın byte[] hali
    // Encoding.UTF8.GetString -> byte dizisini okunabilir stringe çeviriyoruz.
    var message = Encoding.UTF8.GetString(e.Body.ToArray());

    // Gelen mesajı ekrana yazdırıyoruz
    Console.WriteLine("Gelen Mesaj: " + message);


    
    channel.BasicAckAsync(e.DeliveryTag, multiple: false);// mesajın kuyruktan silinmesini sağlar oto silinmedi biz haber verdik.

    // Event’in async olduğu için Task.CompletedTask döndürmek zorundayız.
    return Task.CompletedTask;
};

