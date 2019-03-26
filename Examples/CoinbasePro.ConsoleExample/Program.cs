using System.Threading.Tasks;

namespace CoinbasePro.ConsoleExample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            System.Console.Title = "Console Example";
            var host = new ConsoleHost(args);

            await host.StartAsync();

            await host.StopAsync();
        }
    }
}
