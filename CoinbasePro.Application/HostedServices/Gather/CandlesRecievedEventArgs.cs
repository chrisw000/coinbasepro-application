using System;
using CoinbasePro.Application.Data.Models;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather
{
    public enum CandleSource
    {
        RestCall,
        DatabaseStrategyPreLoad,
        DatabaseOverlayHistoric
    }

    public delegate void CandlesReceivedEventHandler(object sender, CandlesReceivedEventArgs e);

    public class CandlesReceivedEventArgs : EventArgs
    {
        public MarketFeedSettings MarketFeedSettings { get; set; }
        public TimeSeries TimeSeries { get; set; }

        public CandleSource CandleSource { get; set; }

        public CandlesReceivedEventArgs(MarketFeedSettings settings, TimeSeries series, CandleSource candleSource)
        {
            MarketFeedSettings = settings;
            TimeSeries = series;
            CandleSource = candleSource;
        }
    }
}
