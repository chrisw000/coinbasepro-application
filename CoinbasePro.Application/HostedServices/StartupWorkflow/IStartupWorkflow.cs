using System.Threading.Tasks;

namespace CoinbasePro.Application.HostedServices
{
    public interface IStartupWorkflow
    {
        TaskCompletionSource<bool> SocketMonitor { get; }

        TaskCompletionSource<bool> OrderMonitor { get; }

        TaskCompletionSource<bool> AccountMonitor { get; }

        TaskCompletionSource<bool> CandleMonitor { get; }

        TaskCompletionSource<bool> StrategyMonitor { get; }

        TaskCompletionSource<bool>  OverlayMonitor { get; }
    }
}