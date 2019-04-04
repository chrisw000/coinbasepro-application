using System;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.Exceptions;
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

        public CandleMonitorFeeds(ProductType productId, CandleGranularity granularity, bool hasOverlay = true, DateTime? tradeFromUtc = null)
        {
            if (tradeFromUtc != null && tradeFromUtc.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(tradeFromUtc), tradeFromUtc.Value);
            }

            ProductId = productId;
            Granularity = granularity;
            TradeFromUtc = tradeFromUtc;
            HasOverlay = hasOverlay;
        }
    }
}
