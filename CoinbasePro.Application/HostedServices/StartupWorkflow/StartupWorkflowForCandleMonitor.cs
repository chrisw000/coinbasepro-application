using System.Threading.Tasks;

namespace CoinbasePro.Application.HostedServices
{
    public class StartupWorkflowForCandleMonitorOnly : IStartupWorkflow
    {
        public TaskCompletionSource<bool> SocketMonitor { get; }

        public TaskCompletionSource<bool> OrderMonitor { get; }

        public TaskCompletionSource<bool> AccountMonitor { get; }

        public TaskCompletionSource<bool> CandleMonitor { get; }

        public TaskCompletionSource<bool> StrategyMonitor { get; }

        public TaskCompletionSource<bool> OverlayMonitor { get; }

        
        public StartupWorkflowForCandleMonitorOnly()
        {
            // See this link for very detailed explanation of how TaskCompletionSource can block
            // https://blogs.msdn.microsoft.com/seteplia/2018/10/01/the-danger-of-taskcompletionsourcet-class/

            SocketMonitor = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            SocketMonitor.SetResult(true);
            StrategyMonitor = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StrategyMonitor.SetResult(true);
            OverlayMonitor = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            OverlayMonitor.SetResult(true);

            OrderMonitor = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            AccountMonitor = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            CandleMonitor  = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}