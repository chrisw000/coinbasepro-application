using System.Collections.Generic;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Shared.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.Csv
{
    public class CsvCandleProvider : ICandleProvider
    {
        private readonly AppSetting _appSettings;
        private readonly ILogger<CsvCandleDataSource> _logger;
        private readonly Dictionary<ProductType, CsvLastRunDataStore> _lastRun = new Dictionary<ProductType, CsvLastRunDataStore>();
        
        public IDictionary<MarketFeedSettings, ICandleDataSource> DataStores { get; } = new Dictionary<MarketFeedSettings, ICandleDataSource>();

        public CsvCandleProvider(IOptions<AppSetting> appSettings, ILogger<CsvCandleDataSource> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public ICandleDataSource Load(MarketFeedSettings settings)
        {
            if (settings == null)
                return null;

            if (!_lastRun.ContainsKey(settings.ProductId))
                _lastRun.Add(settings.ProductId, CsvLastRunDataStore.Load(_appSettings.CsvPath, settings.ProductId));

            if (DataStores.ContainsKey(settings))
                return DataStores[settings];

            var rc = new CsvCandleDataSource(_appSettings.CsvPath, settings, _lastRun[settings.ProductId], _logger);
            DataStores.Add(settings, rc);
            return rc;
        }
    }
}