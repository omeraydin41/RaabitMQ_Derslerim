// See https://aka.ms/new-console-template for more information

using RabbitMQ.Client; // RabbitMQ kütüphanesini (client) kullanabilmek için ekliyoruz.
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
await channel.QueueDeclareAsync(queue: "birQ", // Kuyruğumuzun adı 'birQ' olsun.
                             durable: true, // **ÖNEMLİ:** RabbitMQ sunucusu yeniden başlasa bile kuyruğun silinmemesini sağlar.
                             exclusive: false, // Bu kuyruğu sadece biz değil,channel den başka değişkenlerde erişsin.subscriber tarafından erişilecek 
                             autoDelete: false, // Son kullanıcı bağlantısı kapansa bile kuyruk otomatik silinmesin.
                             arguments: null); // Ekstra ayar yok.

// Göndereceğimiz mesaj metnini belirliyoruz.
var message = "Hello World!";
// RabbitMQ sadece byte dizisi kabul eder, bu yüzden metni UTF8 formatında byte'a çeviriyoruz.
var bodyMessage = Encoding.UTF8.GetBytes(message);

// Hazırladığımız mesajı kanala yayımlıyoruz (gönderiyoruz).
await channel.BasicPublishAsync(exchange: string.Empty, // Exchange (değiştirici) kullanmıyoruz, direkt kuyruğa göndereceğiz (Default Exchange).
                             routingKey: "birQ", // Exchange kullanmadığımız için, mesajın gideceği kuyruğun adını buraya yazıyoruz.
                             mandatory: false, // Mesajın hedefine ulaşamazsa ne olacağıyla ilgilenmiyoruz.
                                               // Mesajın özelliklerini belirliyoruz.
                             basicProperties: new BasicProperties { Persistent = true },// **ÖNEMLİ:** Mesajın kendisinin de sunucu yeniden başlasa bile kaybolmamasını sağlar.
                             body: bodyMessage); // Göndereceğimiz byte dizisi halindeki mesajımız.

// Konsola mesajın gönderildiğini yazdırarak kullanıcıyı bilgilendiriyoruz.
Console.WriteLine("mesaj gonderildi");

// Mesajın gönderilmesi için 2 saniye bekliyoruz (uygulamanın hemen kapanmaması için).
await Task.Delay(2000);

// Kullanıcıdan bir tuşa basmasını bekle (uygulama penceresinin açık kalması için).
Console.ReadLine();