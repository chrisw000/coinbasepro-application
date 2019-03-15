using System;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using Newtonsoft.Json;

namespace CoinbasePro.Application.Data.Models
{
    public class MarketFeedSettings : IEquatable<MarketFeedSettings>
    {
        public ProductType ProductId { get; set; }
        public CandleGranularity Granularity { get; set; }

        [JsonIgnore]
        public int GranularitySeconds => (int)Granularity;

        public MarketFeedSettings()
        {
            // Required to serialize into IOptions
        }

        public MarketFeedSettings(ProductType id, CandleGranularity granularity)
        {
            ProductId = id;
            Granularity = granularity;
        }

        public override string ToString() => $"{ProductId:G} {Granularity:G}";

        public override bool Equals(object obj)
        {
            if (obj is MarketFeedSettings == false) return false;

            if (ProductId.Equals(((MarketFeedSettings)obj).ProductId) == false) return false;
            if (Granularity.Equals(((MarketFeedSettings)obj).Granularity) == false) return false;
            return true;
        }

        public bool Equals(MarketFeedSettings other)
        {
            return ProductId == other?.ProductId &&
                   Granularity == other.Granularity;
        }

        public override int GetHashCode()
        {
            var hashCode = -162454816;
            hashCode = hashCode * -1521134295 + ProductId.GetHashCode();
            hashCode = hashCode * -1521134295 + Granularity.GetHashCode();
            return hashCode;
        }
    }
}