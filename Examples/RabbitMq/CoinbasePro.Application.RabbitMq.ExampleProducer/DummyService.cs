using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.HostedServices;
using CoinbasePro.Application.HostedServices.Gather;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using Microsoft.Extensions.Logging;
using NodaTime;
using TA4N;

namespace CoinbasePro.Application.RabbitMq.ExampleProducer
{
    public class DummyService : AbstractHostedServiceProvider, IDummyService
    {
        private readonly ICandleProducer _producer;
        private readonly ILogger<DummyService> _logger;

        public DummyService(ICandleProducer producer, ILogger<DummyService> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public int Delay => (1000 * 5); // every 5 seconds send a message

        public override async Task StartupAsync()
        {
            _logger.LogDebug("Example Producer DummyService Startup Running");

            _producer.StartUp();

            _logger.LogDebug("Example Producer DummyService Startup Complete");

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Example Producer DummyService Stop Running");

            _producer.Stop();

            _logger.LogDebug("Example Producer DummyService Stop Complete");

            await Task.CompletedTask;
        }

        public override async Task DoPeriodicWorkAsync()
        {
            _logger.LogDebug("Example Producer DummyService Doing Periodic Work");

            // make something up to send
            _producer.Send(new CandlesReceivedEventArgs(
                new MarketFeedSettings(ProductType.BtcUsd, CandleGranularity.Hour1), 
                new TimeSeries("dummy", new List<Tick>{new Tick(LocalDateTime.FromDateTime(DateTime.Now), 100, 110, 90, 112, 3 )}),
                CandleSource.RestCall));

            await Task.CompletedTask;
        }

    }
}