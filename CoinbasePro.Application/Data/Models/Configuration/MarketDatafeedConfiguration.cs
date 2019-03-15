using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinbasePro.Application.Data.Models.Configuration
{
    public class MarketDatafeedConfiguration : IEntityTypeConfiguration<MarketDatafeed>
    {
        public void Configure(EntityTypeBuilder<MarketDatafeed> builder)
        {
            builder.Property(e => e.LastUpdatedUtc)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(e => e.ProductId)
                .HasConversion<string>();

            builder.Property(e => e.Granularity)
                .HasConversion<string>();
        }
    }
}