using System;
using System.Collections.Generic;
using CoinbasePro.Application.Data.Models;

namespace CoinbasePro.Application.HostedServices.Gather
{
    public interface ICandleMonitorData
    {
        MarketFeedSettings Settings { get; }
        DateTime LastRunUtc { get; }
        DateTime LastUpdatedUtc { get; }
        bool IsFastUpdate { get; }
        bool HasOverlay { get; }
        DateTime NextRunUtc { get; }
    }

    public interface ICandleMonitor : IHostedServiceProvider, ICandleMonitorAsViewModel
    {
        event CandlesReceivedEventHandler CandlesReceived;
    }

    public interface ICandleMonitorAsViewModel
    {
        IReadOnlyCollection<ICandleMonitorData> CandleMonitorData { get; }
    }
}