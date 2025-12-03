
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();



//await channel.QueueDeclareAsync(
//    queue: "ikinci.que",
//    durable: false,
//    exclusive: false,
//    autoDelete: false,
//    arguments: null
//);



//daha once kanal üzerindn kuyruk deklare ederken şimdi exchange deklare ediyoruz
//Fanout exchange hiç fitrelem yapmadan tüm bağlı kuyruklara mesaj gönderir
await channel.ExchangeDeclareAsync//burda bir exchange declare edildi 
    (
     "logs-fanout",//Declare edilen exchange ismi 
     type: ExchangeType.Fanout,//fanout tipinde exchange deklare ettik
     durable: true // RabbitMQ restart olsa bile exchange kalıcı olsun
    );
//Fanout exchange olduğundan kuyruk declare etme işelmini subsciber tarafında yapıyoruz
var list = Enumerable.Range(1, 20).ToList();

foreach (var x in list)
{
    var message = $" {x}. log !";
    var bodyMessage = Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(//mesaj yaınlamak için kullanılan yapı 
        //exchange: "",
        exchange: "logs-fanout", //fanout exchange ismini belirtiyoruz
        routingKey: "",//kuyruk işlemleri subsda olduğundan boş bırakılır 
        mandatory: false,//Eğer exchange’e bağlı hiçbir kuyruk yoksa, mesaj discard edilir (kaybolur)
        body: bodyMessage
    );
    //basicProperties: null mesajla birlikte ek meta veri (header, content type, delivery mode vb.) göndermek için kullanılır.
    //Eğer null verilirse, hiçbir özel özellik gönderilmez, mesaj RabbitMQ’ya saf veri olarak iletilir.


        Console.WriteLine($"mesaj gönderildi : {message}");
    await Task.Delay(2000);
}

Console.ReadLine();

