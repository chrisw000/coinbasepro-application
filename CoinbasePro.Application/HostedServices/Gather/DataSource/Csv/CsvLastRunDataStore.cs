using System;
using System.IO;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using CoinbasePro.Shared.Utilities.Extensions;
using Newtonsoft.Json;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.Csv
{
    public class CsvLastRunDataStore
    {
        private readonly string _csvPath;
        private readonly string _fullPath;

        private DateTime _minutes1;
        private DateTime _minutes5;
        private DateTime _minutes15;
        private DateTime _hour1;
        private DateTime _hour6;
        private DateTime _hour24;

        private CsvLastRunDataStore(string fullPath)
        {
            _minutes1 = DateTime.UtcNow.AddYears(-3);
            _minutes5 = DateTime.UtcNow.AddYears(-3);
            _minutes15 = DateTime.UtcNow.AddYears(-3);
            _hour1 = DateTime.UtcNow.AddYears(-5);
            _hour6 = DateTime.UtcNow.AddYears(-5);
            _hour24 = DateTime.UtcNow.AddYears(-5);

            // This could be better... don't have direct access to the AppSettings out of DI
            // so needs to juggle the path data around
            _fullPath = fullPath;
            _csvPath = new FileInfo(_fullPath).Directory.FullName;
        }

        private CsvLastRunDataStore(CsvLastRunStoreSerializer store, string path)
        {
            _minutes1 = store.Minutes1;
            _minutes5 = store.Minutes5;
            _minutes15 = store.Minutes15;
            _hour1 = store.Hour1;
            _hour6 = store.Hour6;
            _hour24 = store.Hour24;
            _fullPath = path;
            _csvPath = new FileInfo(_fullPath).Directory.FullName;
        }

        internal DateTime Get(CandleGranularity candleGranularity)
        {
            switch (candleGranularity)
            {
                case CandleGranularity.Minutes1:
                    return _minutes1;

                case CandleGranularity.Minutes5:
                    return _minutes5;

                case CandleGranularity.Minutes15:
                    return _minutes15;

                case CandleGranularity.Hour1:
                    return _hour1;

                case CandleGranularity.Hour6:
                    return _hour6;

                case CandleGranularity.Hour24:
                    return _hour24;

                default:
                    throw new ArgumentOutOfRangeException(nameof(candleGranularity), candleGranularity, null);
            }
        }

        internal void Set(CandleGranularity candleGranularity, DateTime utc)
        {
            switch (candleGranularity)
            {
                case CandleGranularity.Minutes1:
                    _minutes1 = utc;
                    break;
                case CandleGranularity.Minutes5:
                    _minutes5 = utc;
                    break;
                case CandleGranularity.Minutes15:
                    _minutes15 = utc;
                    break;
                case CandleGranularity.Hour1:
                    _hour1 = utc;
                    break;
                case CandleGranularity.Hour6:
                    _hour6 = utc;
                    break;
                case CandleGranularity.Hour24:
                    _hour24 = utc;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(candleGranularity), candleGranularity, null);
            }
        }

        internal static CsvLastRunDataStore Load(string csvPath, ProductType productId)
        {
            var fullPath = Path.Combine(csvPath, $"{productId.GetEnumMemberValue()}-rundates.json");

            if (!File.Exists(fullPath))
                return new CsvLastRunDataStore(fullPath);

            var loadValues = JsonConvert.DeserializeObject<CsvLastRunStoreSerializer>(File.ReadAllText(fullPath));
            return new CsvLastRunDataStore(loadValues, fullPath);
        }

        internal void Save(TimeSeries series, MarketFeedSettings settings)
        {
            // Save the csv data
            CsvTimeSeries.Save(_csvPath, series);
            // Save the last run json for next restart
            Set(settings.Granularity, series.LastTick.EndTime.InUtc().ToDateTimeUtc());
            File.WriteAllText(_fullPath, JsonConvert.SerializeObject(new CsvLastRunStoreSerializer(this)));
        }

        // Load and save the data via this so we don't have to expose the actual values via 
        // when in general use...
        private class CsvLastRunStoreSerializer
        {
            public DateTime Minutes1 { get; set; }
            public DateTime Minutes5 { get; set; }
            public DateTime Minutes15 { get; set; }
            public DateTime Hour1 { get; set; }
            public DateTime Hour6 { get; set; }
            public DateTime Hour24 { get; set; }

            public CsvLastRunStoreSerializer(CsvLastRunDataStore store)
            {
                Minutes1 = store._minutes1;
                Minutes5 = store._minutes5;
                Minutes15 = store._minutes15;
                Hour1 = store._hour1;
                Hour6 = store._hour6;
                Hour24 = store._hour24;
            }
        }
    }
}
