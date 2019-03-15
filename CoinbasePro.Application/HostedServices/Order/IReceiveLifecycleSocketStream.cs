using CoinbasePro.WebSocket.Models.Response;

namespace CoinbasePro.Application.HostedServices.Order
{
    public interface IReceiveLifecycleSocketStream
    {
        void WebSocket_OnReceivedReceived(object sender, WebfeedEventArgs<Received> e);
        void WebSocket_OnOpenReceived(object sender, WebfeedEventArgs<Open> e);
        void WebSocket_OnDoneReceived(object sender, WebfeedEventArgs<Done> e);
        void WebSocket_OnMatchReceived(object sender, WebfeedEventArgs<Match> e);
    }
}