using CoinbasePro.Application.Data.Models.Configuration;
using CoinbasePro.Application.Data.Query.Configuration;
using LayerCake.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoinbasePro.Application.Data
{
    // Temp rename to avoid clash with full object whilst code moved into github
    public class CryptoDbX : CryptoXContext
    {
        public CryptoDbX(DbContextOptions<CryptoDbX> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Turn off the warnings on DateTime conversion
            optionsBuilder.ConfigureWarnings(o => o.Ignore(RelationalEventId.ValueConversionSqlLiteralWarning));
        }

        // Built in converters are listed here:
        // https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#built-in-converters
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Using classes to separate the customisation / conversions etc. more:
            // https://scottsauber.com/2017/09/11/customizing-ef-core-2-0-with-ientitytypeconfiguration/
            modelBuilder.ApplyConfiguration(new MarketDatafeedConfiguration());
            modelBuilder.ApplyConfiguration(new MarketDatapointConfiguration());

            // Query - for Stored Procedures etc.
            modelBuilder.ApplyConfiguration(new CandleMonitorFeedsConfiguration());

        }
    }
}
