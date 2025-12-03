using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();







await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: false);

var consumer = new AsyncEventingBasicConsumer(channel);//RabbitMQ mesajlaşma sisteminde mesajları asenkron (eşzamansız) olarak tüketmek için kullanılan bir sınıftır.

var qname = "direct-queue-name-Critical";//kuyruk adı 
await channel.BasicConsumeAsync(qname,//kuyruk adı 
    false,//mesajlar işlenmeden kaybolmasın 
    consumer//hangı tuketıcının mesajları alacağını 
    );//tuketılecek olan kuyruğun adı 

Console.WriteLine("loglar dinleniyor");

consumer.ReceivedAsync += (sender, e) =>//Kuyruğa bir mesaj geldiğinde tetiklenecek olan olay işleyicisini (event handler) tanımlar.
                                        //Mesaj geldiği anda bu asenkron metot çalıştırılır.
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());//Gelen mesajın içeriği (e.Body) byte dizisi halindedir.
                                                            //Bu dizi, UTF8 kodlaması kullanılarak okunabilir metin (string) haline getirilir.
    Console.WriteLine("Gelen mesaj: " + message);


    File.AppendAllText("log-critical.txt", message + "\n");//mesajı texy dosyasına yazdırır


    channel.BasicAckAsync(e.DeliveryTag, false);//Mesajın başarıyla işlendiğini RabbitMQ sunucusuna bildiren asenkron metottur.
                                                //er mesajın benzersiz olan dağıtım etiketidir. RabbitMQ, hangi mesajın onaylandığını bu etiket üzerinden anlar.

    return Task.CompletedTask;//işlem tamamlandımı onu doner 
};
Console.ReadLine();
