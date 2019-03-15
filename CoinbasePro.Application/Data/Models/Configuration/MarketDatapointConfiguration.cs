using System;
using CoinbasePro.Application.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LayerCake.Data.Models
{
    public class MarketDatapointConfiguration : IEntityTypeConfiguration<MarketDatapoint>
    {
        public void Configure(EntityTypeBuilder<MarketDatapoint> builder)
        {
            builder.Property(e => e.EndDatetime)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }
}