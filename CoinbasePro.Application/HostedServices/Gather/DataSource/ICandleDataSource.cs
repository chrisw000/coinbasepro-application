using System;
using System.Threading.Tasks;
using CoinbasePro.Application.Data.Models;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource
{
    public interface ICandleDataSource
    {
        MarketFeedSettings Settings { get;}
        DateTime LastUpdatedUtc { get; set; }
        void Save(TimeSeries series);


        Task<TimeSeries> Load();
        Task<TimeSeries> Load(DateTime? fromUtc);
        Task<TimeSeries> Load(DateTime? fromUtc, DateTime? toUtc);
    }
}
