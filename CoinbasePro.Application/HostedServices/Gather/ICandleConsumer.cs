namespace CoinbasePro.Application.HostedServices.Gather
{
    public interface ICandleConsumer
    {
        event CandlesReceivedEventHandler CandlesReceived;
        void StartUp();
        void Stop();
    }
}