using System;
using System.Collections.Generic;
using System.Linq;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application.Data.Query.CandleMonitor
{
    public class SqlServerCandleMonitorFeed : ICandleMonitorFeedProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SqlServerCandleMonitorFeed(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IEnumerable<CandleMonitorFeeds> GetFeeds()
        {
            IEnumerable<CandleMonitorFeeds> rc;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CryptoDbX>();

                rc = dbContext
                    .Query<CandleMonitorFeeds>().AsNoTracking()
                    .FromSql("candle_monitor_feeds @environment={0}",
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")).ToList();
            }

            return rc;
        }
    }
}