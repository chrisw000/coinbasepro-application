using System;
using System.Collections;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;

namespace CoinbasePro.Application.Data.Query
{
    /// <summary>
    /// Populated from stored_procedure candle_monitor_feeds
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CandleMonitorFeeds
    {
        public ProductType ProductId { get; private set; }
        public CandleGranularity Granularity { get; private set; }
        public DateTime? TradeFromUtc { get; private set; }
        public bool HasOverlay { get; private set; }

        private MarketFeedSettings _marketFeedSettings;

        public MarketFeedSettings MarketFeedSettings => _marketFeedSettings ?? (_marketFeedSettings = new MarketFeedSettings(ProductId, Granularity));

        public CandleMonitorFeeds()
        {
            // empty constructor for EF
        }

        // Used by the CSV provider
        public CandleMonitorFeeds(ProductType productType, CandleGranularity granularity, bool hasOverlay = true, DateTime? tradeFromUtc = null)
        {
            if (tradeFromUtc != null && tradeFromUtc?.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    $"CandleMonitorFeeds({nameof(tradeFromUtc)}) needs to be in UTC, have received: {tradeFromUtc?.Kind} on {tradeFromUtc:O}");
            }

            ProductId = productType;
            Granularity = granularity;
            TradeFromUtc = tradeFromUtc;
            HasOverlay = hasOverlay;
        }
    }
}
