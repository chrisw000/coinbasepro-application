using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinbasePro.Application.Data.Query.Configuration
{
    public class CandleMonitorFeedsConfiguration : IQueryTypeConfiguration<CandleMonitorFeeds>
    {
        public void Configure(QueryTypeBuilder<CandleMonitorFeeds> builder)
        {
            builder.Property(e => e.ProductId)
                .HasConversion<string>();

            builder.Property(e => e.Granularity)
                .HasConversion<string>();

            builder.Property(e => e.TradeFromUtc)
                .HasColumnName("trade_from_utc")
                .HasConversion(v => v, v => DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

            builder.Property(e => e.HasOverlay)
                .HasColumnName("has_overlay")
                .HasConversion<int>();
        }
    }
}
