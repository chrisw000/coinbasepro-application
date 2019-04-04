using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoinbasePro.Application.Data;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.Exceptions;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using CoinbasePro.Exceptions;
using CoinbasePro.Services.Products.Models;
using CoinbasePro.Shared.Types;
using CoinbasePro.Shared.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using TA4N;

namespace CoinbasePro.Application.HostedServices.Gather
{
    public sealed class CandleMonitor : AbstractHostedServiceProvider, ICandleMonitor
    {
        private readonly ICoinbaseProClient _client;
        private readonly IStartupWorkflow _startupWorkflow;
        private readonly ICandleProvider _provider;
        private readonly ICandleMonitorFeedProvider _candleMonitorFeedProvider;
        private readonly ICandleProducer _candleProducer;
        private readonly AppSetting _appSetting;
        private readonly ILogger<CandleMonitor> _logger;

        private readonly ConcurrentDictionary<MarketFeedSettings, CandleMonitorData> _state = new ConcurrentDictionary<MarketFeedSettings, CandleMonitorData>();
        
        // Expose data for the Gather Controller as a ReadOnlyCollection, just the properties being displayed, read only
        public IReadOnlyCollection<ICandleMonitorData> CandleMonitorData => _state.Values.OrderBy(d=>d.Settings.ProductId).ThenBy(d=>d.Settings.GranularitySeconds).ToList();

        public int Delay => (1000 * 1 * 60);
        
        public CandleMonitor(ICoinbaseProClient client
            , IStartupWorkflow startupWorkflow
            , ICandleProvider candleProvider
            , ICandleMonitorFeedProvider candleMonitorFeedProvider
            , ICandleProducer candleProducer
            , IOptions<AppSetting> appSetting
            , ILogger<CandleMonitor> logger)
        {
            _client = client;
            _startupWorkflow = startupWorkflow;
            _provider = candleProvider;
            _candleMonitorFeedProvider = candleMonitorFeedProvider;
            _candleProducer = candleProducer;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public override async Task StartupAsync()
        {
            await Task.WhenAll(
                _startupWorkflow.OverlayMonitor.Task, // Allow overlay runners to be setup, ready to receive events;
                _startupWorkflow.StrategyMonitor.Task,           // Allow the strategy runners to be setup, ready to receive events
                _startupWorkflow.SocketMonitor.Task              // Wait till the socket events are all fired up;
            );

            _logger.LogDebug("CandleMonitor Startup Running");

            // The Overlay & Strategy Runners are all Setup, ready to receive Ticks via the .CandlesReceived event
            // This proc tells us all the Markets to monitor periodically, and the ones with a TradeFromUtc should be
            // polled live. This TradeFromUtc is the MIN(_) data should there be multiple runners for the same market.
            // If a Strategy requires a secondary market (for overlay data into ML model) then the secondary market is
            // populated as live data as well (even if there is no runner with it as a primary market.
            // It also returns the markets with *active* overlay data, to ensure we poll to collect it (either live or periodically)

            foreach (var item in _candleMonitorFeedProvider.GetFeeds())
            {
                var candleMonitorData = new CandleMonitorData(item, _provider, _appSetting);

                // TradeFrom.HasValue is the "fast" markets being trades, so pre-load their data
                if (item.TradeFromUtc.HasValue)
                {
                    await LoadStoredCandles(candleMonitorData, item.TradeFromUtc.Value, CandleSource.DatabaseStrategyPreLoad);
                }

                if (item.HasOverlay)
                {
                    // Pre-load everything to ensure calculations work out ok
                    var allDataPointTime = new DateTime(2015, 1, 1).AsUtc();
                    await LoadStoredCandles(candleMonitorData, allDataPointTime, CandleSource.DatabaseOverlayHistoric);
                }

                _state.AddOrUpdate(item.MarketFeedSettings, candleMonitorData, (s, d) => candleMonitorData);

                _logger.LogTrace("CandleMonitor.StartupAsync for {settings} have completed pre-loading candles", item.MarketFeedSettings);
            }

            _logger.LogDebug("CandleMonitor Startup Complete");
            _startupWorkflow.CandleMonitor.TrySetResult(true);
        }

        private async Task LoadStoredCandles(CandleMonitorData candleMonitorData, DateTime? from, CandleSource candleSource)
        {
            // Pre load anything we've already got and fire it off on the event
            var series = await candleMonitorData.DataSource.Load(from);

            // Fire the new candles event
            _logger.LogTrace("{settings} CandleMonitor.StartupAsync firing CandlesReceivedEvent for source {candleSource}", candleMonitorData.Settings, candleSource);

            try
            {
                _candleProducer.Send(new CandlesReceivedEventArgs(candleMonitorData.Settings, series, candleSource));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{settings} CandleMonitor.StartupAsync.OnCandlesReceived threw an error", candleMonitorData.Settings);
            }
        }

        public override async Task DoPeriodicWorkAsync()
        {
            _logger.LogTrace("CandleMonitor.DoPeriodicWorkAsync Started");
          
            foreach(var dataStore in _state.Values.OrderByDescending(s=>s.Settings.GranularitySeconds))
            {
                try
                {
                    await RunForGranularity(dataStore);
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"RunForGranularity threw on {dataStore.Settings}");
                }
            }

            _logger.LogTrace("CandleMonitor.DoPeriodicWorkAsync Finished");
        }

        private async Task RunForGranularity(CandleMonitorData currentState)
        {
            if (currentState.NextRunUtc >= DateTime.UtcNow)
            {
                _logger.LogTrace("CandleMonitor delaying {settings}, next run date is  {NextRunDate}", currentState.Settings, currentState.NextRunUtc);
                return;
            }

            if (currentState.IsRunning)
            {
                _logger.LogTrace("{settings} is still running, skipping this heartbeat", currentState.Settings);
                return;
            }

            currentState.SetRunning();

            var utcNow = DateTime.UtcNow; // Store this so we're using the same "now" value for longer running batches
            var periodStartUtc = currentState.DataSource.LastUpdatedUtc;
            if (periodStartUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(periodStartUtc), periodStartUtc);
            }

            _logger.LogTrace("CandleMonitor {settings} has data up to {lastUpdated}, getting more...", currentState.Settings, periodStartUtc);

            var maxBatch = currentState.BatchSize;
            var periodEndUtc = periodStartUtc.AddSeconds((maxBatch - 1) * 300 * currentState.Settings.GranularitySeconds);

            // Bring large ranges back to now..
            if (periodEndUtc >= utcNow)
            {
                periodEndUtc = utcNow;
                // Then check to ensure there is at least one full candle of data to ask for
                if (periodStartUtc.AddSeconds(currentState.Settings.GranularitySeconds) > periodEndUtc)
                {
                    _logger.LogDebug("{settings} Skipping... no full candle before utcNow", currentState.Settings);
                    currentState.StopRunning(periodEndUtc);
                    return;
                }
            }

            IList<Candle> candles;

            try
            {
                _logger.LogDebug("{settings} calling GetHistoricRates REST for {periodStartUtc:O} till {periodEndUtc:O}", currentState.Settings, periodStartUtc, periodEndUtc);
                candles = await _client.ProductsService.GetHistoricRatesAsync(currentState.Settings.ProductId, periodStartUtc, periodEndUtc, currentState.Settings.Granularity);
            }
            catch (Exception e)
            {
                if (e.InnerException is CoinbaseProHttpException inner)
                {
                    _logger.LogError(inner, "{settings} HTTP Error {StatusCode}", currentState.Settings, inner.StatusCode);
                }
                else
                {
                    _logger.LogError(e, "{settings} Error", currentState.Settings);
                }
                currentState.StopRunning(periodEndUtc);
                return;
            }

            // Exclude candles that end in the future, to ensure we only store complete candle data
            var series = BuildTimeSeries(currentState.Settings.ProductId,
                                            candles,
                                            currentState.Settings.GranularitySeconds,
                                            periodStartUtc, utcNow);

            if (series.TickCount == 0)
            {
                _logger.LogDebug("{settings} No candles in TimeSeries", currentState.Settings);

                if (periodEndUtc > DateTime.UtcNow.AddDays(-1))
                {
                    _logger.LogTrace("{settings} Finishing as caught up to utcNow minus 1 day", currentState.Settings);
                    currentState.StopRunning(periodEndUtc);
                    return;
                }

                // Allow running through void periods when catching up, or first run etc.
                _logger.LogTrace("{settings} Try next period in case of a void period", currentState.Settings);

                currentState.DataSource.LastUpdatedUtc = periodEndUtc; // This saves the data to db
                currentState.StopRunning(periodEndUtc);
                await DoPeriodicWorkAsync();
                return;
            }

            // Save out the TimeSeries values to the data store (CSV or SQL), including the MarketDataFeed.LastUpdatedDateTime value
            currentState.DataSource.Save(series);

            _logger.LogInformation("{settings} Saved for {TickCount} candles, ending {utc:O}", currentState.Settings, series.TickCount, series.LastTick.EndTime.InUtc().ToDateTimeUtc());

            // Fire the new candles event-------------
            _logger.LogTrace("{settings} CandleMonitor firing event start", currentState.Settings);

            try
            {
                _candleProducer.Send(new CandlesReceivedEventArgs(currentState.Settings, series, CandleSource.RestCall));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{settings} CandleMonitor.RunForGranularity.OnCandlesReceived threw an error", currentState.Settings);
            }
            
            _logger.LogTrace("{settings} CandleMonitor firing event end", currentState.Settings);
            // ---------------------------------------

            currentState.StopRunning(periodEndUtc);
        }

        private static TimeSeries BuildTimeSeries(ProductType productId, IList<Candle> candles, int periodSeconds, DateTime? loadFrom, DateTime? loadUntil)
        {
            if (candles == null) throw new ArgumentNullException(nameof(candles));

            if (loadFrom != null && loadFrom.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(loadFrom), loadFrom.Value);
            }
            if (loadUntil != null && loadUntil.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentNotUtcException(nameof(loadUntil), loadUntil.Value);
            }

            var ticks = new List<Tick>();

            // Sort .Time ascending
            foreach (var line in candles.OrderBy(l => l.Time))
            {
                if (loadFrom.HasValue && line.Time < loadFrom.Value) continue;
                // line.Time is the Start Time, so add on the period to find the end time
                if (loadUntil.HasValue && line.Time.AddSeconds(periodSeconds) > loadUntil.Value) continue;

                var end = line.Time.AddSeconds(periodSeconds);
                var pattern = InstantPattern.ExtendedIso;
                var parseResult = pattern.Parse($"{end:o}");
                var instant = parseResult.GetValueOrThrow();
                var endDate = instant.InUtc().LocalDateTime;

                ticks.Add(new Tick(Period.FromSeconds(periodSeconds)
                    , endDate
                    , line.Open
                    , line.High
                    , line.Low
                    , line.Close
                    , line.Volume));
            }

            return new TimeSeries($"{productId.GetEnumMemberValue()}-{periodSeconds:G}", ticks);
        }
    }
}
