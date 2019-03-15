using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// Put this directly into the the https://github.com/dougdellolio/coinbasepro-csharp/tree/master/CoinbasePro/Network namespace
// ReSharper disable once CheckNamespace
namespace CoinbasePro.Network.HttpClient
{
    public class RateLimitedHttpClient : IHttpClient
    {
        private readonly IHttpClient _client = new HttpClient();
        private readonly CancellationToken cancel = CancellationToken.None;
        private readonly TimeSpanSemaphore semaphore;

        // ReSharper disable once UnusedMember.Local
        private RateLimitedHttpClient()
        {
        }

        public RateLimitedHttpClient(int limit, double periodInSeconds)
        {
            semaphore = new TimeSpanSemaphore(limit, TimeSpan.FromSeconds(periodInSeconds));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            return await semaphore.RunAsync(_client.SendAsync, httpRequestMessage, cancel);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            return await semaphore.RunAsync(_client.SendAsync, httpRequestMessage, cancellationToken);
        }

        public Task<string> ReadAsStringAsync(HttpResponseMessage httpRequestMessage)
        {
            return _client.ReadAsStringAsync(httpRequestMessage);
        }
    }
}