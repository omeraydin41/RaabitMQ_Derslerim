using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();



//dosya yolundan 3 farklı kuyruk deklare edeceğiz 
//3 farklı kuyruk olsun bu yuzden kuyruk adlarını Random atayacağız 

var queue = await channel.QueueDeclareAsync();//channel uzerınden kuyruk decclar ettık 
var queueName = queue.QueueName;//ve QueueName RANDOM KUYRUK ADI VERİR


await channel.QueueBindAsync(// Kuyruğu bir exchange'e bağlamayı asenkron şekilde başlatır.
    queue: queueName, // Bağlanacak kuyruk adı (subscriber'ın oluşturduğu random kuyruk).
    exchange: "logs-fanout",// Mesajların yayınlandığı exchange (fanout türü).
    routingKey: "",
    null// Fanout'ta kullanılmadığı için boş string verilir.
);// Bind işlemini tamamlar.


//BİND : uygulama kapandığında kuyrukta kapanır //kuyrukta declare edebılırdık 
//kuyruk declare etmedik çünku uygulama her ayağı kalktıpında random isimle kuyruk BİND olacak uygulama kapandıpında kuyruk silinecek
//eğer yukruk kalıcı olunması ıstenırse declare edilebilir


await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(queueName, false, consumer);//burad tuketılecek olan kutuk adı random olarak değiştirildi 

Console.WriteLine("loglar dinleniyor");

consumer.ReceivedAsync += (sender, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Console.WriteLine("Gelen mesaj: " + message);

    channel.BasicAckAsync(e.DeliveryTag, false);

    return Task.CompletedTask;
};
Console.ReadLine();
