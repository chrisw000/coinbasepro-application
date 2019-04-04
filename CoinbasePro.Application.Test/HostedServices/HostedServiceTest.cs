using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinbasePro.Application.HostedServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CoinbasePro.Application.Test.HostedServices
{
    [TestFixture]
    public class HostedServiceTest
    {
        [Test]
        public async Task CountCalls_ShutDownWithCancel()
        {
            var mockProvider = new Mock<IHostedServiceProvider>();
            mockProvider.SetupGet(o => o.Delay).Returns(1000); // call provider every second
            
            var fakeLogger = NUnitOutputLogger.Create<IHostedServiceProvider>();

            var subject = new HostedService<IHostedServiceProvider>(mockProvider.Object, fakeLogger);

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(4150);

            await Task.Run(() =>
            {
                subject.StartAsync(tokenSource.Token);
                tokenSource.Token.WaitHandle.WaitOne();
            });

            await Task.Delay(250);

            mockProvider.Verify(o => o.StartupAsync(), Times.Exactly(1));
            mockProvider.Verify(o => o.DoPeriodicWorkAsync(), Times.Exactly(5));
            mockProvider.Verify(o => o.StopAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CountCalls_ShutDownGracefully()
        {
            var mockProvider = new Mock<IHostedServiceProvider>();
            mockProvider.SetupGet(o => o.Delay).Returns(1000); // call provider every second
            
            var fakeLogger = NUnitOutputLogger.Create<IHostedServiceProvider>();

            var subject = new HostedService<IHostedServiceProvider>(mockProvider.Object, fakeLogger);

            await subject.StartAsync(CancellationToken.None);

            await Task.Delay(2500);

            await subject.StopAsync(CancellationToken.None);
            
            mockProvider.Verify(o => o.StartupAsync(), Times.Exactly(1));
            mockProvider.Verify(o => o.DoPeriodicWorkAsync(), Times.Exactly(3));
            mockProvider.Verify(o => o.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
