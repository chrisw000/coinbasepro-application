using System.Threading.Tasks;

namespace CoinbasePro.Application.RabbitMq.ExampleConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.Title = "RabbitMq Consumer";
            var host = new ConsoleHost(args);

            await host.StartAsync();

            await host.StopAsync();

            System.Console.WriteLine("final press");
            System.Console.ReadKey();
        }
    }
}
