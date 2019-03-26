using System.Threading;
using System.Threading.Tasks;
using CoinbasePro.Application.HostedServices;
using CoinbasePro.Application.HostedServices.Gather;
using Microsoft.Extensions.Logging;

namespace CoinbasePro.Application.RabbitMq.ExampleConsumer
{
    public class DummyService : AbstractHostedServiceProvider, IDummyService
    {
        private readonly ICandleConsumer _consumer;
        private readonly ILogger<DummyService> _logger;

        public DummyService(ICandleConsumer consumer, ILogger<DummyService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        public int Delay => (1000 * 60 * 5);

        public override async Task StartupAsync()
        {
            _logger.LogDebug("Example Consumer DummyService Startup Running");

            _consumer.StartUp();
            _consumer.CandlesReceived += _consumer_CandlesReceived;

            _logger.LogDebug("Example Consumer DummyService Startup Complete");

            await Task.CompletedTask;
        }

        private void _consumer_CandlesReceived(object sender, CandlesReceivedEventArgs e)
        {
            _logger.LogInformation("Example Consumer Dummy Service received {TickCount} ticks for {MarketFeedSetting}. {@CandlesReceivedEventArgs}", 
                e.TimeSeries.TickCount,
                e.MarketFeedSettings,
                e);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Example Consumer DummyService Stop Running");

            _consumer.CandlesReceived -= _consumer_CandlesReceived;
            _consumer.Stop();

            _logger.LogDebug("Example Consumer DummyService Stop Complete");

            await Task.CompletedTask;
        }

        public override async Task DoPeriodicWorkAsync()
        {
            _logger.LogDebug("Example Consumer DummyService Doing Periodic Work");
            await Task.CompletedTask;
        }

    }
}