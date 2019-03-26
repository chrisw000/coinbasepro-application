using System.Threading.Tasks;

namespace CoinbasePro.Application.RabbitMq.ExampleProducer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.Title = "RabbitMq Producer";
            var host = new ConsoleHost(args);
            
            await host.StartAsync();

            await host.StopAsync();

            System.Console.WriteLine("final press");
            System.Console.ReadKey();
        }
    }
}
