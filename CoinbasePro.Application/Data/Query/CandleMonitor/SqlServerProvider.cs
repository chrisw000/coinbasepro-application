using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application.Data.Query.CandleMonitor
{
    public class SqlServerProvider : ICandleMonitorFeedProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SqlServerProvider(IServiceScopeFactory serviceScopeFactory)
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