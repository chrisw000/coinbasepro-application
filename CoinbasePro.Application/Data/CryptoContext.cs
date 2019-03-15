using CoinbasePro.Application.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinbasePro.Application.Data
{
    // Temp rename to avoid clash with full object whilst code moved into github
    public class CryptoXContext : DbContext
    {
        public CryptoXContext()
        {
        }

        public CryptoXContext(DbContextOptions<CryptoDbX> options)
            : base(options)
        {
        }

        public virtual DbSet<MarketDatafeed> MarketDatafeed { get; set; }
        public virtual DbSet<MarketDatapoint> MarketDatapoint { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.1-servicing-10028");

            modelBuilder.Entity<MarketDatafeed>(entity =>
            {
                entity.ToTable("market_datafeed");

                entity.Property(e => e.MarketDatafeedId).HasColumnName("market_datafeed_id");

                entity.Property(e => e.Currency1)
                    .IsRequired()
                    .HasColumnName("currency1")
                    .HasMaxLength(6);

                entity.Property(e => e.Currency2)
                    .IsRequired()
                    .HasColumnName("currency2")
                    .HasMaxLength(6);

                entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");

                entity.Property(e => e.Granularity)
                    .IsRequired()
                    .HasColumnName("granularity")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.IsGatheredPeriodically).HasColumnName("is_gathered_periodically");

                entity.Property(e => e.LastUpdatedUtc)
                    .HasColumnName("last_updated_datetime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("('01/01/001')");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnName("productId")
                    .HasMaxLength(12)
                    .HasComputedColumnSql("([currency1]+[currency2])");
            });

            modelBuilder.Entity<MarketDatapoint>(entity =>
            {
                entity.HasKey(e => e.MarketDatapointId)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("market_datapoint");

                entity.HasIndex(e => new { e.High, e.EndDatetime })
                    .HasName("market_datapoint_high");

                entity.HasIndex(e => new { e.MarketDatafeedId, e.EndDatetime })
                    .HasName("IX_market_datapoint")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.MarketDatapointId).HasColumnName("market_datapoint_id");

                entity.Property(e => e.Close)
                    .HasColumnName("close")
                    .HasColumnType("decimal(18, 10)");

                entity.Property(e => e.EndDatetime)
                    .HasColumnName("end_datetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.High)
                    .HasColumnName("high")
                    .HasColumnType("decimal(18, 10)");

                entity.Property(e => e.Low)
                    .HasColumnName("low")
                    .HasColumnType("decimal(18, 10)");

                entity.Property(e => e.MarketDatafeedId).HasColumnName("market_datafeed_id");

                entity.Property(e => e.Open)
                    .HasColumnName("open")
                    .HasColumnType("decimal(18, 10)");

                entity.Property(e => e.Volume)
                    .HasColumnName("volume")
                    .HasColumnType("decimal(18, 10)");

                entity.HasOne(d => d.MarketDatafeed)
                    .WithMany(p => p.MarketDatapoint)
                    .HasForeignKey(d => d.MarketDatafeedId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_market_datapoint_market_datafeed");
            });
        }
    }
}
