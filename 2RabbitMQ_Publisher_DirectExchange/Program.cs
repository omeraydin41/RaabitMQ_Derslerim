using RabbitMQ.Client;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    public enum LogsName //Enum, C#’ta “sabit değerleri isimlendirerek bir araya toplayan özel bir veri tipi”dir.
    {
        Critical = 1,//1 değeri criticalı ifade eder 
        Error = 2,//2 değeri erroru 
        Warning = 3,//3 warnıngi 
        Info = 4//4 infoyu ifade eder 
    }
    //biz isimler uzerınden değil sayılar uzerınden işlem yapacağız 
    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory();
        factory.Uri = new Uri("amqps://djufkfnr:Ljr4OZWoMz8PemaiC35bJjXUjN6WYXRD@moose.rmq.cloudamqp.com/djufkfnr");

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();






        await channel.ExchangeDeclareAsync(//channel uzerınden exchange declare edilmei 
            "logs-direct",//exchange ismi 
            type: ExchangeType.Direct,//exchange tipi 
            durable: true //rabbitmq yenıden başlatılsa bıle exchange kalıcı olsunmu 
        );


        //her bir log için kuyruk  ve root oluşturulacak 


        foreach (var x in Enum.GetNames(typeof(LogsName)).ToList())//Enum.GetNames(typeof(LogsName) enumun isimlerini ver ve x e ata
        {//4 kere donecek ve her donduğunde her mesaj turune ait bir KUYRUK OLUŞTURACAK 

            var queueName = $"direct-queue-name-{x}"; //ve kutruk adlarını dınamık olarak belirtelim .x değeri enumda ısım değererini tutar 

            await channel.QueueDeclareAsync(
                queue: queueName,//dınamık olarak gelen q ismi 
                durable: true,//rabbıtmq yenıden başlasa bile kuyruk kalıcı olsunmu 
                exclusive: false,//bu değişken harıcı başka değişkenlerde erişsinmi .mesela subs tarafından erişilenilmek istenebilir
                autoDelete: false,//kuyruk otomatık sıllınsınmı 
                arguments: null//Kuyruk için ekstra argüman (özellik) yoktur.
            );

            await channel.QueueBindAsync//bind bağlamak demek KUYRUKLA EXCHANGEYİ BAĞLAR.
                (queueName,//KUYRUK ADI 
                "logs-direct", //EXCHANGE ADI 
                x//ROOTKEY YONLENDIRME ANAHTARI : MESELA X=1 OLDUĞUNDA ERROR KUYRUĞUNA GIT
                );
            //QueueBindAsync() metodunun temel amacı, bir Kuyruğu(Queue) belirli bir Exchange'e bağlamaktır.
            //Bu bağlantı olmadan, Exchange'e gelen mesajlar o kuyruğa yönlendirilemez.
        }

        var list = Enumerable.Range(1, 20).ToList();

        foreach (var item in list)
        {
            LogsName log = (LogsName)Random.Shared.Next(1, 5);//LOGSname turune çevir neyi =1 iile 5 arasındakı gelecek sayıyı critical warnıng info eror turune

            var message = $"{log} log!";//rabbıt mq ya gonerrilecek mesaj 
            var bodyMessage = Encoding.UTF8.GetBytes(message);//çevirildi 

            await channel.BasicPublishAsync(//mesaj gonderme methodu 
                exchange: "logs-direct",//kuyruk adı 
                routingKey: $"route-{log}",//1 ile 4 arasındakı sayıdır bu da 4 mesaj turune denk gelşr 
                mandatory: false,
                body: bodyMessage//mesaj 
            );

            Console.WriteLine($"log gönderildi : {message}");// 4 mesaj turunun 
            await Task.Delay(2000);
        }

        Console.ReadLine();
    }
}
