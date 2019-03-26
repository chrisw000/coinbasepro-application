using System;
using System.Threading.Tasks;
using CoinbasePro.Application.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CoinbasePro.Application.RabbitMq.ExampleProducer
{
    public class ConsoleHost
    {
        private readonly IHost _host;

        public ServiceProvider ServiceProvider { get; private set; }

        public ConsoleHost(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("CoinbasePro.Application.RabbitMq", LogEventLevel.Verbose)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            
            var builder = new HostBuilder().UseConsoleLifetime();

            builder.ConfigureHostConfiguration(config => {
                config.AddEnvironmentVariables(); // pulls environment name from Properties/launchSettings.json
            });

            builder.ConfigureLogging((hostingContext, config) => {
                config.AddSerilog();
            });

            builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    var env = hostContext.HostingEnvironment;
                    configurationBuilder.SetBasePath(env.ContentRootPath);

                    // These files are copied to bin in the build, so needs rebuild to push changes into app
                    configurationBuilder.AddJsonFile("appsettings.json", false, true);
                    configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
                    configurationBuilder.AddEnvironmentVariables();

                    if (args != null) configurationBuilder.AddCommandLine(args);
                });

            builder.ConfigureServices((hostContext, services) =>
                {
                    services.AddApplication(hostContext.Configuration);
                    services.AddRabbitMq(hostContext.Configuration);

                    services.AddSingleton<IDummyService, DummyService>();
                    services.AddHostedService<HostedService<IDummyService>>();

                    // All calls to services.Configure *MUST* have come before this
                    ServiceProvider = services.BuildServiceProvider();
            });

            _host = builder.Build(); 
        }

        public async Task StartAsync()
        {
            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            System.Console.WriteLine("Press any key to finish.....");
            await System.Console.In.ReadLineAsync();

            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(2));
            }
        }
    }
}
