using System.Collections.Generic;
using CoinbasePro.Application.Data.Query;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.Csv
{
    public class CsvCandleMonitorFeed : ICandleMonitorFeedProvider
    {
        private readonly IEnumerable<CandleMonitorFeeds> _rc;

        public CsvCandleMonitorFeed(IEnumerable<CandleMonitorFeeds> feeds)
        {
            _rc = feeds;
        }

        public IEnumerable<CandleMonitorFeeds> GetFeeds()
        {
            return _rc;
        }
    }
}