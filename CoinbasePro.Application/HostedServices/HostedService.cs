using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoinbasePro.Application.HostedServices
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HostedService<T> : BackgroundService
                    where T : class, IHostedServiceProvider
    {
        private readonly IHostedServiceProvider _provider;
        private readonly ILogger<T> _logger;
        private readonly string _typeName;

        public HostedService(T provider, ILogger<T> logger)
        {
            _provider = provider;
            _logger = logger;
            _typeName = typeof(T).Name;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting HostedService<{HostedServiceName}>", _typeName);

                await _provider.StartupAsync();

                _logger.LogDebug("HostedService<{HostedServiceName}>.StartupAsync complete", _typeName);

            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "HostedService<{HostedServiceName}>.StartupAsync threw an exception", _typeName);
            }


            try
            {
                _logger.LogDebug("HostedService<{HostedServiceName}> Periodic Work loop started", _typeName);

                stoppingToken.Register(() =>
                    _logger.LogInformation("Stopping HostedService<{HostedServiceName}> background task is stopping due to cancellation", _typeName));

                while (!stoppingToken.IsCancellationRequested)
                {
                    await _provider.DoPeriodicWorkAsync();
                    await Task.Delay(_provider.Delay, stoppingToken);
                }

                _logger.LogWarning("Stopping HostedService<{HostedServiceName}> background task", _typeName);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "HostedService<{HostedServiceName}> Main Loop threw an exception", _typeName);
            }

        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Run your graceful clean-up actions
                await _provider.StopAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "HostedService<{HostedServiceName}>.StopAsync threw an exception", _typeName);
            }
        }
    }
}