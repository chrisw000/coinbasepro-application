using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoinbasePro.Application.Data.Models;
using CoinbasePro.Application.Data.Query;
using CoinbasePro.Application.HostedServices;
using CoinbasePro.Application.HostedServices.Gather;
using CoinbasePro.Application.HostedServices.Gather.DataSource;
using CoinbasePro.Services.Products;
using CoinbasePro.Services.Products.Models;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using TA4N;

namespace CoinbasePro.Application.Test.HostedServices.Gather
{
    [TestFixture]
    public class CandleMonitorTest
    {
        public struct CandleSourceEventSummary
        {
            public int DatabaseStrategyPreLoad { get; set; }
            public int DatabaseOverlayHistoric { get; set; }
            public int RestCall { get; set; }
        }

        private Mock<ICoinbaseProClient> mockCoinbaseProClient;
        private IStartupWorkflow fakeWorkflow;
        private ISetup<ICandleProducer> setupMockCandleProducer;

        private CandleSourceEventSummary iCountCandleSourceEvents;

        
        private CandleMonitor ResetSubject(CandleMonitorFeeds feedData)
        {
            iCountCandleSourceEvents= new CandleSourceEventSummary();
            
            mockCoinbaseProClient = new Mock<ICoinbaseProClient>();

            fakeWorkflow = StartupWorkflow.ForCandleMonitorOnly();
            
            var mockCandleDataSource = new Mock<ICandleDataSource>();
            mockCandleDataSource.Setup(o => o.Load(DateTime.UtcNow)).ReturnsAsync(Mock.Of<TimeSeries>());
            mockCandleDataSource.SetupProperty(o => o.LastUpdatedUtc, DateTime.UtcNow.AddDays(-100));

            var mockCandleProvider = new Mock<ICandleProvider>();
            mockCandleProvider.Setup(o => o.Load(Mock.Of<MarketFeedSettings>()).LastUpdatedUtc).Returns(DateTime.UtcNow.AddDays(-100));
            mockCandleProvider.SetupGet(o => o.DataStores[It.IsAny<MarketFeedSettings>()]).Returns(mockCandleDataSource.Object);

            var mockCandleMonitorFeedProvider = new Mock<ICandleMonitorFeedProvider>();
            mockCandleMonitorFeedProvider.Setup(o => o.GetFeeds()).Returns(new List<CandleMonitorFeeds> { feedData });

            var mockCandleProducer = new Mock<ICandleProducer>();
            setupMockCandleProducer = mockCandleProducer.Setup(o => o.Send(It.IsAny<CandlesReceivedEventArgs>()));

            var fakeAppSettings = Options.Create(new AppSetting());
            var fakeLogger = NUnitOutputLogger.Create<CandleMonitor>();

            var subject = new CandleMonitor(
                mockCoinbaseProClient.Object,
                fakeWorkflow,
                mockCandleProvider.Object,
                mockCandleMonitorFeedProvider.Object,
                mockCandleProducer.Object,
                fakeAppSettings,
                fakeLogger);

            return subject;
        }

        // TradeFromUtc raises CandleSource.DatabaseStrategyPreLoad events from the producer
        // HasOverlay   raises CandleSource.DatabaseOverlayHistoric events from the producer
        [TestCaseSource(typeof(CandleMonitorTestData), nameof(CandleMonitorTestData.TestCases))]
        public async Task<CandleSourceEventSummary> StartUp_Calls_Producer_With_CandleSource(CandleMonitorFeeds feedData)
        {
            var subject = ResetSubject(feedData);

            // Use a callback on the mockCandleProducer.Send, in order to count how many times the Producer is called for each CandleSource
            setupMockCandleProducer.Callback(CountCandleSourceEvents());

            await subject.StartupAsync();

            return iCountCandleSourceEvents;
        }

        [Test]
        public async Task StartUp_Workflow_Set()
        {
            var feedData = new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(), It.IsAny<bool>(), DateTime.UtcNow);

            var subject = ResetSubject(feedData);

            await subject.StartupAsync();

            Assert.That(fakeWorkflow.CandleMonitor.Task.IsCompleted, Is.EqualTo(true));
        }

        [Test]
        public async Task DoPeriodicWork_CountCalls()
        {
            var feedData = new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(), It.IsAny<bool>(), DateTime.UtcNow);

            var subject = ResetSubject(feedData);

            // Use a callback on the mockCandleProducer.Send, in order to count how many times the Producer is called for each CandleSource
            setupMockCandleProducer.Callback(CountCandleSourceEvents());

            var mockProductsService = new Mock<IProductsService>();
            mockProductsService.Setup(o => o.GetHistoricRatesAsync(
                    It.IsAny<ProductType>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CandleGranularity>()))
                .ReturnsAsync(new List<Candle>
                {
                    new Candle {Close=100.0M, High=110.0M, Low=90M, Open=95.0M, Time=DateTime.UtcNow, Volume=10.0M}
                });

            mockCoinbaseProClient.Setup(o => o.ProductsService).Returns(mockProductsService.Object);

            await subject.StartupAsync();

            await subject.DoPeriodicWorkAsync();

            mockProductsService.Verify(o => 
                    o.GetHistoricRatesAsync(It.IsAny<ProductType>(), 
                                            It.IsAny<DateTime>(), 
                                            It.IsAny<DateTime>(), 
                                            It.IsAny<CandleGranularity>()), 
                    Times.Exactly(1));

            Assert.That(iCountCandleSourceEvents.RestCall, Is.EqualTo(1));
        }

        private Action<CandlesReceivedEventArgs> CountCandleSourceEvents()
        {
            return eventArgs =>
            {
                switch (eventArgs.CandleSource)
                {
                    case CandleSource.DatabaseStrategyPreLoad:
                        iCountCandleSourceEvents.DatabaseStrategyPreLoad++;
                        break;
                    case CandleSource.DatabaseOverlayHistoric:
                        iCountCandleSourceEvents.DatabaseOverlayHistoric++;
                        break;
                    case CandleSource.RestCall:
                        iCountCandleSourceEvents.RestCall++;
                        break;
                    default:
                        Assert.Fail("Missing CandleSource handling in the test");
                        break;
                }
            };
        }

        private class CandleMonitorTestData
        {
            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(
                        new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(), 
                            true, DateTime.UtcNow)).Returns(new CandleSourceEventSummary{DatabaseStrategyPreLoad= 1, DatabaseOverlayHistoric= 1, RestCall= 0});

                    yield return new TestCaseData(
                        new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(), 
                            false, DateTime.UtcNow)).Returns(new CandleSourceEventSummary{DatabaseStrategyPreLoad= 1, DatabaseOverlayHistoric= 0, RestCall= 0});

                    yield return new TestCaseData(
                        new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(),
                            true, null)).Returns(new CandleSourceEventSummary{DatabaseStrategyPreLoad= 0, DatabaseOverlayHistoric= 1, RestCall= 0});

                    yield return new TestCaseData(
                        new CandleMonitorFeeds(It.IsAny<ProductType>(), It.IsAny<CandleGranularity>(),
                            false, null)).Returns(new CandleSourceEventSummary{DatabaseStrategyPreLoad= 0, DatabaseOverlayHistoric= 0, RestCall= 0});
                }
            }
        }
    }
}
