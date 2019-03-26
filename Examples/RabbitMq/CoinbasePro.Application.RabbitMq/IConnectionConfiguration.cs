using System.Collections.Generic;
using RabbitMQ.Client;

namespace CoinbasePro.Application.RabbitMq
{
    public interface IConnectionConfiguration
    {
        /// <summary>
        /// HostName    192.168.1.21
        /// Port:       5672 for regular connections
        ///             5671 for connections that use TLS
        /// </summary>
        IList<AmqpTcpEndpoint> EndPoints { get; }
        string UserName { get; }
        string Password { get; }
        string VirtualHost { get; }
    }
}