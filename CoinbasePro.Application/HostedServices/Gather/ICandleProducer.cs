namespace CoinbasePro.Application.HostedServices.Gather
{
    public interface ICandleProducer
    {
        void StartUp();
        void Send(CandlesReceivedEventArgs e);
        void Stop();
    }
}