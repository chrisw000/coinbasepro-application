using System.Collections.Generic;
using CoinbasePro.Application.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.SqlServer
{
    public class SqlServerCandleProvider : ICandleProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IDictionary<MarketFeedSettings, ICandleDataSource> DataStores { get; } = new Dictionary<MarketFeedSettings, ICandleDataSource>();

        public SqlServerCandleProvider(IServiceScopeFactory serviceFactory)
        {
            _serviceScopeFactory = serviceFactory;
        }

        public ICandleDataSource Load(MarketFeedSettings settings)
        {
            if (DataStores.ContainsKey(settings))
                return DataStores[settings];

            var rc = new SqlServerCandleDataSource(settings, _serviceScopeFactory);
            DataStores.Add(settings, rc);
            return rc;
        }
    }
}