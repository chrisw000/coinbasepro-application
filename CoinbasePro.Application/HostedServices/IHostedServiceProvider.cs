using System.Threading;
using System.Threading.Tasks;

namespace CoinbasePro.Application.HostedServices
{
    public interface IHostedServiceProvider
    {
        int Delay { get; }

        Task DoPeriodicWorkAsync();
        Task StartupAsync();
        Task StopAsync(CancellationToken stoppingToken);
    }
}