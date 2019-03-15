using System;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.Data.Query;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using CoinbasePro.Services.Products.Types;

namespace CoinbasePro.Application.HostedServices.Gather
{
    public class CandleMonitorData : ICandleMonitorData
    {
        internal int BatchSize { get; }
        internal bool IsRunning { get; private set; }
        internal ICandleDataSource DataSource { get; }

        public MarketFeedSettings Settings { get; }
        public DateTime LastRunUtc { get; private set; }
        public DateTime LastUpdatedUtc => DataSource.LastUpdatedUtc;

        public bool IsFastUpdate { get; }
        public bool HasOverlay { get; }
    
        public DateTime NextRunUtc
        {
            get
            {
                if (IsFastUpdate) return LastUpdatedUtc.AddSeconds(Settings.GranularitySeconds);

                switch (Settings.Granularity)
                {
                    case CandleGranularity.Minutes1:
                        return LastUpdatedUtc.AddMinutes(239);
                    case CandleGranularity.Minutes5:
                        return LastUpdatedUtc.AddHours(19);
                    case CandleGranularity.Minutes15:
                        return LastUpdatedUtc.AddHours(37);
                    case CandleGranularity.Hour1:
                        return LastUpdatedUtc.AddHours(76);
                    case CandleGranularity.Hour6:
                        return LastUpdatedUtc.AddHours(121);
                    case CandleGranularity.Hour24:
                        return LastUpdatedUtc.AddDays(7);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private CandleMonitorData()
        {
            // hide use
        }

        public CandleMonitorData(CandleMonitorFeeds item, ICandleProvider candleProvider)
        {
            Settings = item.MarketFeedSettings;

            var p = candleProvider.Load(Settings); // Just populate the provider DataStores, don't get any data
            
            LastRunUtc = p.LastUpdatedUtc;
            IsFastUpdate = item.TradeFromUtc.HasValue;
            HasOverlay = item.HasOverlay;
            DataSource = candleProvider.DataStores[Settings];

            BatchSize = IsFastUpdate ? 60 : 25;
        }

        internal void SetRunning()
        {
            IsRunning = true;
        }

        internal void StopRunning(DateTime periodEndUtc)
        {
            if (periodEndUtc.Kind != DateTimeKind.Utc)
                throw new Exception("CandleMonitorData.StopRunning(periodEndUtc) needs DateTimeKind==Utc");

            IsRunning = false;
            LastRunUtc = periodEndUtc;
        }
    }
}
