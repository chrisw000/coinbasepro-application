using System.Threading.Tasks;

namespace CoinbasePro.ConsoleExample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new ConsoleHost(args);

            await host.StartAsync();

            await host.StopAsync();
        }
    }
}
