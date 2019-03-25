namespace CoinbasePro.Application.HostedServices.Gather
{
    public class CandleProducerConsumer : ICandleProducer, ICandleConsumer
    {
        // ICandleConsumer
        public event CandlesReceivedEventHandler CandlesReceived;

        #region ICandleProducer
        public void StartUp()
        {
            // Don't need to do anything
        }

        public void Send(CandlesReceivedEventArgs e)
        {
            CandlesReceived?.Invoke(this, e);
        }

        public void Stop()
        {
            // Don't need to do anything
        }
        #endregion
    }
}
