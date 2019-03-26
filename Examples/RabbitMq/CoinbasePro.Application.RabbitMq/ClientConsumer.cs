using System.Text;
using CoinbasePro.Application.HostedServices.Gather;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TA4N;

namespace CoinbasePro.Application.RabbitMq
{
    public class ClientConsumer : Client, ICandleConsumer
    {
        public ClientConsumer(ConnectionConfiguration connectionConfig,
            ChannelConfiguration channelConfig,
            IJsonUtil jsonUtil,
            ILogger<Client> logger) : base(connectionConfig, channelConfig, jsonUtil, logger)
        {

        }

        public event CandlesReceivedEventHandler CandlesReceived;

        public void StartUp()
        {
            Connect();

            if (_isConnected == false)
            {
                _logger.LogWarning("Not starting ClientConsumer eventing");
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var obj = _jsonUtil.DeserializeObject<CandlesReceivedEventArgs>(message);
                CandlesReceived?.Invoke(this, obj);
            };

            _channel.BasicConsume(_channelConfig.QueueName, true, consumer);
        }

        public void Stop()
        {
            Close();
        }
    }
}