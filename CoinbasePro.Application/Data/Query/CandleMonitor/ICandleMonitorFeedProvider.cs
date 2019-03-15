using System.Collections.Generic;

namespace CoinbasePro.Application.Data.Query.CandleMonitor
{
    public interface ICandleMonitorFeedProvider
    {
        IEnumerable<CandleMonitorFeeds> GetFeeds();
    }
}