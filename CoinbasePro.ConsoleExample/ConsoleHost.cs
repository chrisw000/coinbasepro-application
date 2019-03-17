using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoinbasePro.Application;
using CoinbasePro.Application.Data;
using CoinbasePro.Application.Data.Query;
using CoinbasePro.Application.Data.Query.CandleMonitor;
using CoinbasePro.Application.HostedServices;
using CoinbasePro.Application.HostedServices.Gather;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using CoinbasePro.Application.HostedServices.Gather.DataSource.Csv;
//using CoinbasePro.Application.HostedServices.Gather.DataSource.SqlServer;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Network.HttpClient;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CoinbasePro.ConsoleExample
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
                .MinimumLevel.Override("CoinbasePro", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.Azure.KeyVault", LogEventLevel.Verbose)
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
                    configurationBuilder.AddJsonFile("appsettings.json", optional: false, true);
                    configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
                    configurationBuilder.AddEnvironmentVariables();

                    if (args != null) configurationBuilder.AddCommandLine(args);

                    // To enable running locally follow steps here:
                    // https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication
                    // Install Azure CLI v2.0.57 at time of writing (06/02/2019)
                    // from Azure CLI, run:
                    // az login
                    // az account get-access-token
                    // ---> the token now allows the Azure part to work correctly

                    // - commented out for demo
                    //var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    //var keyVaultClient = new KeyVaultClient(
                    //        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                    //var builtConfig = configurationBuilder.Build();

                    //configurationBuilder.AddAzureKeyVault(
                    //    builtConfig["ASPNETCORE_HOSTINGSTARTUP__KEYVAULT__CONFIGURATIONVAULT"],
                    //    keyVaultClient,
                    //    new DefaultKeyVaultSecretManager());
                    
                });

            builder.ConfigureServices((hostContext, services) =>
                {
                    // --------------------------------------------
                    // Coinbase Pro log-on credentials, usually these should be saved in the Azure Key vault so they're never exposed
                    // without Azure, comment out the azureServiceTokenProvider part above and use the app config file directly
                    services.AddSingleton<IAuthenticator>(new Authenticator(hostContext.Configuration["apiKey"]
                                                                , hostContext.Configuration["apiSecret"]
                                                                , hostContext.Configuration["passPhrase"]));

                    // Customise the HttpClient to ensure we don't ever get HTTP 429 rate limit errors
                    services.AddSingleton<IHttpClient>(new RateLimitedHttpClient(3, 2.1));
                    services.AddSingleton<CoinbaseProClient>();

                    // --------------------------------------------
                    // The IStartupWorkflow controls the startup order of services in order
                    // the ensure inter dependencies receive events or data correctly
                    // As all the services are not yet in Github ensure CandleMonitor can still start
                    // so use StartupWorkflowForCandleMonitorOnly
                    services.AddSingleton<IStartupWorkflow, StartupWorkflowForCandleMonitorOnly>();

                    // Usually SQL Server would be used (sql scripts not in Github yet)
                    //services.AddSingleton<ICandleProvider, SqlServerCandleProvider>();
                    //services.AddTransient<ICandleMonitorFeedProvider, SqlServerCandleMonitorFeed>();

                    // Instead use the CSV route for ease of demonstration
                    services.AddSingleton<ICandleProvider, CsvCandleProvider>();
                    // Setup the markets to pull data for
                    services.AddTransient<ICandleMonitorFeedProvider>(sp => new CsvCandleMonitorFeed(new List<CandleMonitorFeeds>()
                                        {
                                            // There is a bug in GDAX.Api.ClientLibrary that causes endless loop  calling REST service
                                            // when the amount of data on GDAX is less than what is trying to be pulled
                                            // I've submitted a buxfix - which will be in the 1.0.28 Nuget version
                                            // for now just pull currencies with at least a year of data
                                            new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Hour1),
                                            new CandleMonitorFeeds(ProductType.EthUsd, CandleGranularity.Hour1),
                                            new CandleMonitorFeeds(ProductType.EthEur, CandleGranularity.Minutes15)
                                        })
                    );

                    // Add the hosted services
                    services.AddSingleton<ICandleMonitor, CandleMonitor>();
                    services.AddHostedService<HostedService<ICandleMonitor>>(); 

                    //// Finish up and add database
                    //    services.AddDbContext<CryptoDbX>(options =>
                    //    {
                    //        options.UseSqlServer(hostContext.Configuration.GetConnectionString("MyDbConnection"),
                    //            // retry https://blogs.msdn.microsoft.com/cesardelatorre/2017/03/26/using-resilient-entity-framework-core-sql-connections-and-transactions-retries-with-exponential-backoff/
                    //            sqlServerOptionsAction: sqlOptions =>
                    //            {
                    //                sqlOptions.EnableRetryOnFailure(
                    //                    maxRetryCount: 5,
                    //                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //                    errorNumbersToAdd: null);
                    //            });
                    //        options.EnableSensitiveDataLogging(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production");
                    //    });

                    services.Configure<AppSetting>(hostContext.Configuration);

                    // All calls to services.Configure *MUST* have come before this
                    ServiceProvider = services.BuildServiceProvider();
            });

            _host = builder.Build();

            //// Automatically perform database migration
            //serviceProvider.GetService<CryptoXContext>().Database.Migrate();
        }

        public async Task StartAsync()
        {
            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            Console.WriteLine("Press any key to finish.....");
            await Console.In.ReadLineAsync();

            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(2));
            }
        }
    }
}
