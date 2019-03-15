using System;

namespace CoinbasePro.Application.Data.Models
{
    // There is also MarketDatapointInfo in Query
    public partial class MarketDatapoint
    {
        public int MarketDatapointId { get; set; }
        public int MarketDatafeedId { get; set; }
        public DateTime EndDatetime { get; set; }
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public decimal? Volume { get; set; }

        public MarketDatafeed MarketDatafeed { get; set; }
    }
}
