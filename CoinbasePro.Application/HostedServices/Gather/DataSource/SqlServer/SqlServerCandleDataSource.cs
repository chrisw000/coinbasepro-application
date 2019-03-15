using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinbasePro.Application.Data;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Shared.Utilities.Extensions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather.DataSource.SqlServer
{
    public class SqlServerCandleDataSource : ICandleDataSource
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _marketDataFeedId;

        private readonly ILogger<SqlServerCandleDataSource> _logger;

        public MarketFeedSettings Settings { get; }

        public SqlServerCandleDataSource(MarketFeedSettings settings, IServiceScopeFactory serviceScopeFactory)
        {
            _scopeFactory = serviceScopeFactory;
            Settings = settings;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                _marketDataFeedId = dbContext.MarketDatafeed.Single(
                    df => df.ExchangeId == 1 &&
                    df.ProductId == Settings.ProductId &&
                    df.Granularity == Settings.Granularity).MarketDatafeedId;

                _logger = scope.ServiceProvider.GetService<ILogger<SqlServerCandleDataSource>>();
            }
        }

        public DateTime LastUpdatedUtc
        {
            get
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                    return dbContext.MarketDatafeed.Single(
                        df => df.MarketDatafeedId == _marketDataFeedId).LastUpdatedUtc;
                }
            }
            set
            {
                if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("LastUpdatedUtc must be saved to SQL Server in UTC", nameof(value));

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                    dbContext.MarketDatafeed.Single(
                        df => df.MarketDatafeedId == _marketDataFeedId).LastUpdatedUtc = value;

                    dbContext.SaveChanges();
                }
            }
        }

        public void Save(TimeSeries series)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                // https://blogs.msdn.microsoft.com/cesardelatorre/2017/03/26/using-resilient-entity-framework-core-sql-connections-and-transactions-retries-with-exponential-backoff/
                var strategy =  dbContext.Database.CreateExecutionStrategy();
                //await strategy.ExecuteAsync(async () =>
                strategy.Execute( () =>
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var points = new List<MarketDatapoint>();

                            for (var i = 0; i < series.TickCount; i++)
                            {
                                var tick = series.GetTick(i);

                                points.Add(new MarketDatapoint
                                {
                                    MarketDatafeedId = _marketDataFeedId,
                                    EndDatetime = tick.EndTime.InUtc().ToDateTimeUtc(),
                                    Open = (decimal) tick.OpenPrice,
                                    High = (decimal) tick.MaxPrice,
                                    Low = (decimal) tick.MinPrice,
                                    Close = (decimal) tick.ClosePrice,
                                    Volume = (decimal) tick.Volume
                                });
                            }

                            dbContext.MarketDatafeed.Single(df => df.MarketDatafeedId == _marketDataFeedId).LastUpdatedUtc = series.LastTick.EndTime.InUtc().ToDateTimeUtc();
                            dbContext.BulkInsert(points, new BulkConfig {PreserveInsertOrder = true});

                            dbContext.SaveChanges();

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "SqlServerCandleDataSource.Save {message} {settings} ", e.Message, Settings);
                            transaction.Rollback();
                        }
                    }

                });
            }
        }

        public async Task<TimeSeries> Load() => await Load(null, null);

        public async Task<TimeSeries> Load(DateTime? fromDateUtc) => await Load(fromDateUtc, null);

        /// <summary>
        /// Inclusive fromUtc, exclusive toUtc
        /// </summary>
        public async Task<TimeSeries> Load(DateTime? fromUtc, DateTime? toUtc)
        {
            if (fromUtc == null) fromUtc = DateTime.UtcNow.AddDays(-400);
            if (toUtc == null) toUtc = DateTime.UtcNow;

            var ticks = new List<Tick>();
            var period = NodaTime.Period.FromSeconds(Settings.GranularitySeconds);

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                foreach(var line in await dbContext.MarketDatapoint.Where(m =>
                                        m.MarketDatafeedId == _marketDataFeedId
                                        && m.EndDatetime >= fromUtc.Value
                                        && m.EndDatetime < toUtc.Value) // inclusive from, exclusive to
                                    .OrderBy(m => m.EndDatetime).ToListAsync())
                {
                    var date = NodaTime.Instant.FromDateTimeUtc(line.EndDatetime).InUtc().LocalDateTime;

                    if (date.InUtc().ToDateTimeUtc() < fromUtc.Value) continue;
                    if (date.InUtc().ToDateTimeUtc() > toUtc.Value) continue;

                    if (line.Open == null || line.Close == null || (line.Volume == null))
                    {
                        _logger.LogWarning("Loading SQL Data, tick value has null; skipping {@Line}", line);
                        continue;
                    }

                    var open = line.Open.Value;
                    var high = line.High ?? open;
                    var low = line.Low ?? open;

                    var close = line.Close.Value;
                    var volume = line.Volume.Value;

                    ticks.Add(new Tick(period, date, open, high, low, close, volume));
                }
                
                return new TimeSeries($"{Settings.ProductId.GetEnumMemberValue()}-{Settings.Granularity:D}", ticks);
            }
        }
    }
}
