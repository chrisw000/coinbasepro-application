using System.Text;
using CoinbasePro.Application.HostedServices.Gather;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Framing;
using TA4N;

namespace CoinbasePro.Application.RabbitMq
{
    public class ClientProducer : Client, ICandleProducer
    {
        private const string TheAppId = "Coinbase.Application";
        private const string TheContentType = "application/json; charset=utf-8";

        private readonly BasicProperties _props;

        public ClientProducer(ConnectionConfiguration connectionConfig,
            ChannelConfiguration channelConfig,
            IJsonUtil jsonUtil,
            ILogger<Client> logger) : base(connectionConfig, channelConfig, jsonUtil, logger)
        {
            _props = new BasicProperties
            {
                AppId = TheAppId,
                ContentType = TheContentType
            };
        }

        public void StartUp()
        {
            Connect();
        }

        public void Send(CandlesReceivedEventArgs message)
        {
            if (_isConnected)
            {
                _channel.BasicPublish(_channelConfig.ExchangeName, 
                    _channelConfig.RoutingKey, 
                    false, 
                    _props,
                    Encoding.UTF8.GetBytes(_jsonUtil.SerializeObject(message)));

                _logger.LogTrace("Message sent on channel {ChannelNumber}",  _channel.ChannelNumber);
            }
            else
            {
                _logger.LogError("Attempt to send {Message} when Client not connected.", message);
            }
        }

        public void Stop()
        {
            Close();
        }
    }
}