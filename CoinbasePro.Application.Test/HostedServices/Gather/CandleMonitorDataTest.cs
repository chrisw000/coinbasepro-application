using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.Data.Query;
using CoinbasePro.Application.HostedServices.Gather;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Moq;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace CoinbasePro.Application.Test.HostedServices.Gather
{
    [TestFixture]
    public class CandleMonitorDataTest
    {
        #region "Settings"
        [Test]
        public void Settings()
        {
            var settings = new MarketFeedSettings(ProductType.BatUsdc, CandleGranularity.Hour1);

            var c = MockCandleMonitorData_ForIsFastUpdate(settings, false, false, null);

            Assert.That(c.Settings, Is.EqualTo(settings));
        }
        #endregion

        #region HasOverlay
        [TestCase(true, true, ExpectedResult = true)]
        [TestCase(false, true, ExpectedResult = false)]
        [TestCase(true, false, ExpectedResult = true)]
        [TestCase(false, false, ExpectedResult = false)]
        public bool HasOverlay(bool hasOverlay, bool appSettingHasOverlayFastUpdate)
        {
            var settings = new MarketFeedSettings(ProductType.BatUsdc, CandleGranularity.Hour1);

            var c = MockCandleMonitorData_ForIsFastUpdate(settings, hasOverlay, appSettingHasOverlayFastUpdate, null);

            return c.HasOverlay;
        }
        #endregion
        
        #region IsFastUpdate
        [TestCaseSource(typeof(IsFastUpdateTestData), nameof(IsFastUpdateTestData.TestCases))]
        public bool IsFastUpdate(bool hasOverlay, bool appSettingHasOverlayFastUpdate, DateTime? tradeFromUtc)
        {
            var settings = new MarketFeedSettings(ProductType.BatUsdc, CandleGranularity.Hour1);

            var c = MockCandleMonitorData_ForIsFastUpdate(settings, hasOverlay, appSettingHasOverlayFastUpdate, tradeFromUtc);

            return c.IsFastUpdate;
        }

        private class IsFastUpdateTestData
        {
            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(true, true, null).Returns(true);
                    yield return new TestCaseData(false, false, null).Returns(false);
                    yield return new TestCaseData(false, true, null).Returns(false);
                    yield return new TestCaseData(true, false, null).Returns(false);
                    yield return new TestCaseData(false, false, null).Returns(false);

                    yield return new TestCaseData(true, true, DateTime.UtcNow).Returns(true);
                    yield return new TestCaseData(false, false, DateTime.UtcNow).Returns(true);
                    yield return new TestCaseData(false, true, DateTime.UtcNow).Returns(true);
                    yield return new TestCaseData(true, false, DateTime.UtcNow).Returns(true);
                    yield return new TestCaseData(false, false, DateTime.UtcNow).Returns(true);
                }
            }  
        }
        #endregion

        private static CandleMonitorData MockCandleMonitorData_ForIsFastUpdate(
                                            MarketFeedSettings settings, 
                                            bool hasOverlay,
                                            bool appSettingHasOverlayFastUpdate, 
                                            DateTime? tradeFromUtc)
        {
            var feed = new CandleMonitorFeeds(settings.ProductId, settings.Granularity, hasOverlay, tradeFromUtc);

            var mockCandleDataSource = new Mock<ICandleDataSource>();

            var mockCandleProvider = new Mock<ICandleProvider>();
            mockCandleProvider.Setup(o => o.Load(settings).LastUpdatedUtc).Returns(DateTime.UtcNow);
            mockCandleProvider.SetupGet(o => o.DataStores[settings]).Returns(mockCandleDataSource.Object);

            var mockAppSetting = new Mock<IAppSettingCandleMonitor>();
            mockAppSetting.SetupProperty(o => o.HasOverlayFastUpdate, appSettingHasOverlayFastUpdate);

            var c = new CandleMonitorData(feed, mockCandleProvider.Object, mockAppSetting.Object);
            return c;
        }
    }
}
