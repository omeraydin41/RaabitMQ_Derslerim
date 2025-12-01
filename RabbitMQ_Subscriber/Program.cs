using RabbitMQ.Client;           // RabbitMQ client kütüphanesi
using RabbitMQ.Client.Events;    // EventingBasicConsumer için gerekli
using System.Text;               // Byte -> string dönüşümü için


var factory = new ConnectionFactory();


factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");


using var connection = await factory.CreateConnectionAsync();


using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "birQ", // Kuyruğumuzun adı 'birQ' olsun.
                             durable: true, // **ÖNEMLİ:** RabbitMQ sunucusu yeniden başlasa bile kuyruğun silinmemesini sağlar.
                             exclusive: false, // Bu kuyruğu sadece biz değil,channel den başka değişkenlerde erişsin.subscriber tarafından erişilecek 
                             autoDelete: false, // Son kullanıcı bağlantısı kapansa bile kuyruk otomatik silinmesin.
                             arguments: null); // Ekstra ayar yok.





// Kanaldan mesajları asenkron şekilde tüketmek için AsyncEventingBasicConsumer sınıfını kullanıyoruz.
// Bu consumer, gelen mesajları Event tabanlı bir mantıkla (ReceivedAsync) dinler.
var consumer = new AsyncEventingBasicConsumer(channel);

// "birQ" isminde kuyruğu tüketmeye (dinlemeye) başlatıyoruz.
// 2. parametre: autoAck (false = manuel onaylama) -> Mesajı biz onaylayacağız.
// consumer: Mesajları teslim edecek tüketici.
await channel.BasicConsumeAsync("birQ", false, consumer);

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

    // Manuel mesaj onayı yapılmazsa mesaj kuyrukta "unacked" kalır.
    // Eğer kanal autoAck=false olduğu için buraya eklemeyi unutursan mesaj silinmez.
    channel.BasicAckAsync(e.DeliveryTag, multiple: false);

    // Event’in async olduğu için Task.CompletedTask döndürmek zorundayız.
    return Task.CompletedTask;
};


//var body = e.Body.ToArray();
//var message = Encoding.UTF8.GetString(body);
//Console.WriteLine("Gelen Mesaj: " + message);
//return Task.CompletedTask;

//consumer received bir eventtir ve bu event için bir method yazmamız gerekiyor.istersek değişkenleride yukrada kullanabılırız 
//Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
//{
//    throw new NotImplementedException();
//}