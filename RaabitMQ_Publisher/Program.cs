// See https://aka.ms/new-console-template for more information

using RabbitMQ.Client; // RabbitMQ kütüphanesini (client) kullanabilmek için ekliyoruz.
using System.Linq;
using System.Text;   // Mesaj metnini byte dizisine çevirmek için gerekli.

// Bağlantı ayarlarını tutacak bir nesne oluşturuyoruz.
var factory = new ConnectionFactory();

// CloudAMQP'den aldığın (veya başka bir cloud/uzak sunucu) bağlantı URI'sini ayarlıyoruz.
// Bu URI, sunucu adresi, port, kullanıcı adı ve şifre gibi tüm bilgileri içerir.
factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

// 'using var' ile bağlantı işimiz bitince otomatik kapatılmasını sağlıyoruz.
using var connection = await factory.CreateConnectionAsync();

// Kurulan bağlantı üzerinden bir 'kanal' açıyoruz. 
// Tüm mesajlaşma işlemleri (kuyruk oluşturma, mesaj gönderme) bu kanal üzerinden yapılır.
using var channel = await connection.CreateChannelAsync();

// Kanala bir kuyruk tanımlamasını asenkron olarak gönderiyoruz.
await channel.QueueDeclareAsync(queue: "ikinci.que", // Kuyruğumuzun adı 'birQ' olsun.
                             durable: false, // **ÖNEMLİ:** RabbitMQ sunucusu yeniden başlasa bile kuyruğun silinmemesini sağlar.
                             exclusive: false, // Bu kuyruğu sadece biz değil,channel den başka değişkenlerde erişsin.subscriber tarafından erişilecek 
                             autoDelete: false, // Son kullanıcı bağlantısı kapansa bile kuyruk otomatik silinmesin.
                             arguments: null); // Ekstra ayar yok.


var list = Enumerable.Range(1, 20).ToList(); // 1'den 20'ye kadar sayılardan oluşan liste oluştur

foreach (var x in list) // Listedeki her sayı için döngü
{
    var message = $"{x}.mesaj gönderildi!"; // bilgi mesajı 
    var bodyMessage = Encoding.UTF8.GetBytes(message); // Mesajı byte dizisine çevir

    await channel.BasicPublishAsync(
        exchange: string.Empty, // Default exchange kullan
        routingKey: "ikinci.que", // Mesajın gideceği kuyruğun adı
        mandatory: false, // Mesaj hedefe ulaşamazsa hata dikkate alınmasın
        basicProperties: new BasicProperties { Persistent = true }, // Mesaj kalıcı olsun
        body: bodyMessage // Byte dizisi halindeki mesajı gönder
    );

    Console.WriteLine($"mesaj gonderildi{message}"); // Konsola mesaj gönderildiğini yaz
    await Task.Delay(2000); // 2 saniye bekle, uygulamanın hemen kapanmasını önle
}

// Kullanıcıdan bir tuşa basmasını bekle (uygulama penceresinin açık kalması için).
Console.ReadLine();