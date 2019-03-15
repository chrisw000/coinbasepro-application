using System.Threading;
using System.Threading.Tasks;

namespace CoinbasePro.Application.HostedServices
{
    public abstract class AbstractHostedServiceProvider
    {
        public virtual async Task StartupAsync()
        {
            await Task.CompletedTask;
        }

        public virtual async Task DoPeriodicWorkAsync()
        {
            await Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }
    }
}