namespace CoinbasePro.Application.RabbitMq
{
    public class ChannelConfiguration : IChannelConfiguration, IValidateStartUp
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }

        public void Validate()
        {
            // Validate() doesn't get called when starting up via ConsoleHost, is fired 
            if(string.IsNullOrEmpty(ExchangeName)) throw new SettingValidationException(nameof(ChannelConfiguration), nameof(ExchangeName), "Cannot be null or empty");
            if(string.IsNullOrEmpty(QueueName)) throw new SettingValidationException(nameof(ChannelConfiguration), nameof(QueueName), "Cannot be null or empty");
            if(string.IsNullOrEmpty(RoutingKey)) throw new SettingValidationException(nameof(ChannelConfiguration), nameof(RoutingKey), "Cannot be null or empty");
        }
    }
}