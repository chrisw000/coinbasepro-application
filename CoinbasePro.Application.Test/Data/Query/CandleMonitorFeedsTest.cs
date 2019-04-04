using System;
using CoinbasePro.Application.Data.Query;
using CoinbasePro.Application.Exceptions;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using NUnit.Framework;

namespace CoinbasePro.Application.Test.Data.Query
{
    [TestFixture]
    public class CandleMonitorFeedsTest
    {
        private static readonly DateTime utcKind = DateTime.UtcNow;
        private static readonly DateTime unspecifiedKind = DateTime.Now;

        [TestCase(ProductType.BtcUsd, CandleGranularity.Hour1)]
        [TestCase(ProductType.LtcGbp, CandleGranularity.Minutes15)]
        public void EnsureMarketFeedSettings(ProductType productType, CandleGranularity granularity)
        {
            var subject = new CandleMonitorFeeds(productType, granularity, true, null);
            
            Assert.That(subject.Granularity, Is.EqualTo(granularity));
            Assert.That(subject.Granularity, Is.EqualTo(subject.MarketFeedSettings.Granularity));

            Assert.That(subject.ProductId, Is.EqualTo(productType));
            Assert.That(subject.ProductId, Is.EqualTo(subject.MarketFeedSettings.ProductId));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnsureHasOverlay(bool hasOverlay)
        {
            var subject = new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Minutes1, hasOverlay, null);
            
            Assert.That(subject.HasOverlay, Is.EqualTo(hasOverlay));
        }

        [Test]
        public void EnsureTradeFromUtcNull()
        {
            var subject = new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Minutes1, true, null);
            
            Assert.That(subject.TradeFromUtc, Is.Null);
        }

        [Test]
        public void EnsureTradeFromUtcIsUtc()
        {
            Assert.That(utcKind.Kind, Is.EqualTo(DateTimeKind.Utc));

            var subject = new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Minutes1, true, utcKind);
            Assert.That(subject.TradeFromUtc, Is.Not.Null);
            Assert.That(subject.TradeFromUtc.Value, Is.EqualTo(utcKind));
        }

        [Test]
        public void EnsureTradeFromUtcThrows()
        {
            Assert.That(() => new CandleMonitorFeeds(ProductType.BtcUsd, CandleGranularity.Minutes1, true, unspecifiedKind), 
                Throws.Exception
                    .TypeOf<ArgumentNotUtcException>()
                    .With.Property("DateTime")
                    .EqualTo(unspecifiedKind));
        }
    }
}