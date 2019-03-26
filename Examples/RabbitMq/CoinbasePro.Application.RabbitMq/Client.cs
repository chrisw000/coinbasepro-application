using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using TA4N;

namespace CoinbasePro.Application.RabbitMq
{
    public abstract class Client
    {
        private readonly IConnectionConfiguration _connectionConfig;
        protected readonly IChannelConfiguration _channelConfig;
        protected readonly IJsonUtil _jsonUtil;
        protected readonly ILogger<Client> _logger;

        protected bool _isConnected = false;
        private IConnection _connection;
        protected IModel _channel;
        
        protected Client(ConnectionConfiguration connectionConfig, 
            ChannelConfiguration channelConfig,
            IJsonUtil jsonUtil,
            ILogger<Client> logger)
        {
            _connectionConfig = connectionConfig;
            _channelConfig = channelConfig;
            _jsonUtil = jsonUtil;
            _logger = logger;
        }

        protected void Connect()
        {
            try
            {
                /*
                 * When you create a new vhost on a cluster - don't forget to enable a HA-policy for that vhost (even if you don’t have
                 * a HA setup, you will need it for plan changes). Messages will not be synced between nodes without an HA-policy.
                 */

                IEndpointResolver endpointResolver = new DefaultEndpointResolver(_connectionConfig.EndPoints);

                // Builds up url in the format
                // "amqp://guest:guest@192.168.1.1:5672/virtual-host"
                var factory = new ConnectionFactory
                {
                    UserName = _connectionConfig.UserName,
                    Password = _connectionConfig.Password,
                    VirtualHost = _connectionConfig.VirtualHost,
                    HandshakeContinuationTimeout = new TimeSpan(TimeSpan.TicksPerSecond * 5),
                    RequestedConnectionTimeout = 3000
                };

                _logger.LogInformation($"Connecting...");

                _connection = factory.CreateConnection(endpointResolver, "this is my client name");

                _logger.LogInformation(
                    $"Connected to {_connection.Endpoint.HostName}:{_connection.Endpoint.Port}"); // successful and unsuccessful client connection events can be observed in server node logs

                _channel = _connection.CreateModel();

                /*
                 * ExchangeType.Direct  - A message goes to the queue(s) whose binding key exactly matches the routing key of the message.
                 *
                 * ExchangeType.Fanout  - The fanout copies and routes a received message to all queues that are bound to it regardless of
                 *                        routing keys or pattern matching as with direct and topic exchanges. Keys provided will simply be ignored.
                 *                        Fanout exchanges can be useful when the same message needs to be sent to one or more queues with
                 *                        consumers who may process the same message in different ways.
                 *
                 * ExchangeType.Headers - Headers exchanges route based on arguments containing headers and optional values. Headers exchanges
                 *                        are very similar to topic exchanges, but it routes based on header values instead of routing keys.
                 *                        A message is considered matching if the value of the header equals the value specified upon binding.
                 *
                 * ExchangeType.Topic   - Topic exchanges route messages to queues based on wildcard matches between the routing key and
                 *                        the routing pattern specified by the queue binding. Messages are routed to one or many queues based
                 *                        on a matching between a message routing key and this pattern.
                 *                        eg;  crypro.gdax.btc.usd.hour-1    stock.lse.gsk.gbp.hour-24
                 *
                 * Dead Letter Exchange - If no matching queue can be found for the message, the message is silently dropped. RabbitMQ provides
                 *                        an AMQP extension known as the "Dead Letter Exchange" - the dead letter exchange provides functionality
                 *                        to capture messages that are not deliverable.
                 */
                _channel.ExchangeDeclare(_channelConfig.ExchangeName, ExchangeType.Direct);
                _channel.ModelShutdown += Channel_ModelShutdown;

                // Persistent messages and durable queues for a message to survive a server restart = resilient / lower performance
                // for high throughput send transient messages to non-lazy queues

                // Limit queue size with TTL or max-length (queue will discard messages from the head of queue to keep within mex-length)
                _channel.QueueDeclare(_channelConfig.QueueName, true, false, false, null);
                _channel.QueueBind(_channelConfig.QueueName, _channelConfig.ExchangeName,
                    _channelConfig.RoutingKey, null);

                // https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.IModel.html
                _logger.LogInformation(
                    $"Using channel {_channel.ChannelNumber}, which has timeout of {_channel.ContinuationTimeout.TotalSeconds} seconds");

                _isConnected = true;
            }
            catch (BrokerUnreachableException)
            {
                _logger.LogError("Coinbase.Application.RabbitMq.Client.Connect() None of the specified endpoints were reachable; endpoints: {EndPoints} vhost: {VirtualHost}",
                    string.Join(',', _connectionConfig.EndPoints.Select(i => $"{i.HostName}:{i.Port}")),
                    _connectionConfig.VirtualHost);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception during Coinbase.Application.RabbitMq.Client.Connect");
            }
        }

        protected void Close()
        {
            try
            {
                _channel?.Close(); // Shutdown gracefully
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception during Coinbase.Application.RabbitMq.Client.Close for channel {Channel}", _channel);
            }

            try
            {
                _connection?.Close(); // Shutdown gracefully
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception during Coinbase.Application.RabbitMq.Client.Close for connection {Connection}", _connection);
            }

        }
    
        private void Channel_ModelShutdown(object sender, ShutdownEventArgs e)
        {
            string helpText;
            /*
             Certain scenarios are assumed to be recoverable ("soft") errors in the protocol. They render the channel
             closed but applications can open another one and try to recover or retry a number of times.
             TODO: attempt recovery
             */
            switch (e.ReplyCode)
            {
                case 200:
                    // All OK
                    _logger.LogInformation("{ReplyCode} OK Shutdown, {ReplyText}", e.ReplyCode, e.ReplyText);
                    break;

                case 403:
                    helpText = @"
- Accessing a resource the user is not allowed to access will fail with a 403 ACCESS_REFUSED error.

";
                    _logger.LogWarning("{ReplyCode} ACCESS_REFUSED Shutdown Event. {ReplyText}. {HelpText} {ShutdownEventArgs}", e.ReplyCode, e.ReplyText, helpText, e);
                    break;

                case 404:
                    helpText = @"
- Binding a non-existing queue or a non-existing exchange will fail with a 404 NOT_FOUND error
- Consuming from a queue that does not exist will fail with a 404 NOT_FOUND error
- Publishing to an exchange that does not exit will fail with a 404 NOT_FOUND error

";
                    _logger.LogWarning("{ReplyCode} NOT_FOUND Shutdown Event. {ReplyText}. {HelpText} {ShutdownEventArgs}", e.ReplyCode, e.ReplyText, helpText, e);
                    break;

                case 405:
                    helpText = @"
- Accessing an exclusive queue from a connection other than its declaring one will fail with a 405 RESOURCE_LOCKED

";
                    _logger.LogWarning("{ReplyCode} RESOURCE_LOCKED Shutdown Event. {ReplyText}. {HelpText} {ShutdownEventArgs}", 
                        e.ReplyCode, 
                        e.ReplyText, 
                        helpText, 
                        e);
                    break;

                case 406:
                    helpText = @"
- Re declaring an existing queue or exchange with non-matching properties will fail with a 406 PRECONDITION_FAILED error

";
                    _logger.LogWarning("{ReplyCode} PRECONDITION_FAILED Shutdown Event. {ReplyText}. {HelpText} {ShutdownEventArgs}", 
                        e.ReplyCode, 
                        e.ReplyText, 
                        helpText,
                        e);
                    break;

                case 530:
                    // NOT_ALLOWED
                    _logger.LogError("{ReplyCode} NOT_ALLOWED Shutdown Event. {ReplyText}. user: {UserName} vhost: {VirtualHost} {ShutdownEventArgs}", 
                        e.ReplyCode, 
                        e.ReplyText, 
                        _connectionConfig.UserName, 
                        _connectionConfig.VirtualHost, 
                        e );
                    break;

                default:
                    _logger.LogError("{ReplyCode} Unhandled Shutdown Event. {ReplyText}. {ShutdownEventArgs}", 
                        e.ReplyCode
                        , e.ReplyText
                        , e);
                    break;
            }
        }
    }
}