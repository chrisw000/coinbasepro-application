using System;
using System.IO;
using System.Threading.Tasks;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Shared.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.Csv
{
    public class CsvCandleDataSource : ICandleDataSource
    {
        private readonly CsvLastRunDataStore _parent;
        private readonly ILogger _logger;

        public CsvCandleDataSource(MarketFeedSettings settings, CsvLastRunDataStore parent, ILogger<CsvCandleDataSource> logger)
        {
            Settings = settings;
            _parent = parent;
            _logger = logger;
        }
        public MarketFeedSettings Settings { get; }
        
        public DateTime LastUpdatedUtc
        {
            get => _parent.Get(Settings.Granularity);
            set => _parent.Set(Settings.Granularity, value);
        }

        public async Task<TimeSeries> Load() => await Load(null, null);

        public async Task<TimeSeries> Load(DateTime? fromUtc) => await Load(fromUtc, null);

        /// <summary>
        /// Inclusive fromUtc, exclusive toUtc
        /// </summary>
        public async Task<TimeSeries> Load(DateTime? fromUtc, DateTime? toUtc)
        {
            if (fromUtc == null) fromUtc = DateTime.UtcNow.AddDays(-400);
            if (toUtc == null) toUtc = DateTime.UtcNow;

            await Task.CompletedTask; // this is only here because the SQL route reads async

            // CSV Loader - need to share file names
            // - or have a "being loaded" set used by CandleMonitor
            // - and a periodically taken copy for Strategy runs?
            var fileName = $"{Settings.ProductId.GetEnumMemberValue()}-{Settings.Granularity:D}.csv";

            try
            {
                return CsvTimeSeries.LoadSeries(fileName, (int)Settings.Granularity, fromUtc, toUtc);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning($"CsvCandleProvider.Load file not found {fileName} - using empty TimeSeries.");
                return new TimeSeries(fileName);
            }
        }

        public void Save(TimeSeries series)
        {
            _parent.Save(series, Settings);
        }
    }
}
