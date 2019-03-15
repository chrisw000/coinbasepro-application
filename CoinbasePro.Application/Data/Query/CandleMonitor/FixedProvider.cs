using System.Collections.Generic;

namespace CoinbasePro.Application.Data.Query.CandleMonitor
{
    public class FixedProvider : ICandleMonitorFeedProvider
    {
        private readonly IEnumerable<CandleMonitorFeeds> _rc;

        public FixedProvider(IEnumerable<CandleMonitorFeeds> feeds)
        {
            _rc = feeds;
        }

        public IEnumerable<CandleMonitorFeeds> GetFeeds()
        {
            return _rc;
        }
    }
}