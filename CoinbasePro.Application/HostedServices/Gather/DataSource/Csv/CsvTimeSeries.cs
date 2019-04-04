using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoinbasePro.Application.Exceptions;
using CsvHelper;
using NodaTime;
using NodaTime.Text;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.Csv
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CsvTimeSeries
    {
        private static readonly LocalDateTimePattern DateTimePattern = LocalDateTimePattern.CreateWithInvariantCulture("o");

        /// <summary>
        /// Inclusive fromUtc, exclusive toUtc
        /// </summary>
        public static TimeSeries LoadSeries(string fullPath, int periodInSeconds, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"CsvTimeSeries.LoadSeries {fullPath} not found.");
            }

            if (fromUtc != null && fromUtc.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(fromUtc), fromUtc.Value);

            }
            if (toUtc != null && toUtc.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(toUtc), toUtc.Value);
            }

            List<dynamic> lines;
            var ticks = new List<Tick>();

            using (var reader = File.OpenText(fullPath))
            {
                // Reading all lines of the CSV file
                var csvReader = new CsvReader(reader);
                lines = csvReader.GetRecords<dynamic>().ToList();
            }

            var period = Period.FromSeconds(periodInSeconds);

            foreach (var line in lines)
            {
                //Console.WriteLine("Expecting input {0}.", Pattern.Format(new LocalDateTime(2014, 5, 26, 13, 45, 22)));

                ParseResult<LocalDateTime> parseResult = DateTimePattern.Parse(line.date);

                var date = parseResult.GetValueOrThrow();

                // Inclusive from 
                if (fromUtc.HasValue && date.InUtc().ToDateTimeUtc() < fromUtc.Value) continue;
                if (toUtc.HasValue && date.InUtc().ToDateTimeUtc() >= toUtc.Value) continue;

                var open = decimal.Parse(line.open);

                var high = string.IsNullOrEmpty(line.high) 
                    ? (decimal) open 
                    : decimal.Parse(line.high);

                var low = string.IsNullOrEmpty(line.low) 
                    ? (decimal) open 
                    : decimal.Parse(line.low);

                decimal close = decimal.Parse(line.close);
                decimal volume = decimal.Parse(line.volume);

                ticks.Add(new Tick(period, date, open, high, low, close, volume));
            }

            var fi = new FileInfo(fullPath);

            return new TimeSeries(fi.Name, ticks);
        }
        
        public static void Save(string csvPath, TimeSeries series)
        {
            var fullPath = Path.Combine(csvPath, $"{series.Name}.csv");

            var lines = new List<string>();

            if (!File.Exists(fullPath))
            {
                lines.Add("date,open,high,low,close,volume");
            }

            for (var i = 0; i < series.TickCount; i++)
            {
                var tick = series.GetTick(i);

                lines.Add(
                    $"{DateTimePattern.Format(tick.EndTime)},{tick.OpenPrice},{tick.MaxPrice},{tick.MinPrice},{tick.ClosePrice},{tick.Volume}");
            }

            if (!Directory.Exists(csvPath))
            {
                // TODO: logging
                Directory.CreateDirectory(csvPath);
            }

            File.AppendAllLines(fullPath, lines);
        }
    }
}
