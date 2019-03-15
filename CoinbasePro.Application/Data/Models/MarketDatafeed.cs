using System;
using System.Collections.Generic;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;

namespace CoinbasePro.Application.Data.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class MarketDatafeed
    {
        public MarketDatafeed()
        {
            MarketDatapoint = new HashSet<MarketDatapoint>();
        }

        public int MarketDatafeedId { get; set; }
        public int ExchangeId { get; set; }
        public ProductType ProductId { get; set; }
        public string Currency1 { get; set; }
        public string Currency2 { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public CandleGranularity Granularity { get; set; }
        public bool IsGatheredPeriodically { get; set; }

        public ICollection<MarketDatapoint> MarketDatapoint { get; set; }
    }
}
