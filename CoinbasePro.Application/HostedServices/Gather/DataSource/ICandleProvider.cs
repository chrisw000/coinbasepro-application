using System.Collections.Generic;
using CoinbasePro.Application.Data.Models;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource
{
    public interface ICandleProvider
    {
        IDictionary<MarketFeedSettings, ICandleDataSource> DataStores { get; }

        ICandleDataSource Load(MarketFeedSettings settings);
    }
}
