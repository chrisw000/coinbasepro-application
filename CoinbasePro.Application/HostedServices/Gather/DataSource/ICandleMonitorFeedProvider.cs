using System.Collections.Generic;
using CoinbasePro.Application.Data.Query;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource
{
    public interface ICandleMonitorFeedProvider
    {
        IEnumerable<CandleMonitorFeeds> GetFeeds();
    }
}