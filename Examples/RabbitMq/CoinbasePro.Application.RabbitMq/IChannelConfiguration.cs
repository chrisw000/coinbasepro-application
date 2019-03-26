namespace CoinbasePro.Application.RabbitMq
{
    public interface IChannelConfiguration
    {
        string ExchangeName { get; }
        string QueueName { get; }
        string RoutingKey { get; }
    }
}