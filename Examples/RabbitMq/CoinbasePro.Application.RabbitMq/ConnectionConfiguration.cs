using System.Collections.Generic;
using RabbitMQ.Client;

namespace CoinbasePro.Application.RabbitMq
{
    public class ConnectionConfiguration : IConnectionConfiguration, IValidateStartUp
    {
        public IList<AmqpTcpEndpoint> EndPoints { get; set; }
        public string UserName { get; set;}
        public string Password { get; set; }
        public string VirtualHost { get; set;} //  virtual host with empty names are not addressable (default should be (is) "/")

        public void Validate()
        {
            // Validate() doesn't get called when starting up via ConsoleHost, is fired 
            if(EndPoints==null || EndPoints.Count==0) throw new SettingValidationException(nameof(ConnectionConfiguration), nameof(EndPoints), "Cannot be null or empty");
            if(string.IsNullOrEmpty(UserName)) throw new SettingValidationException(nameof(ConnectionConfiguration), nameof(UserName), "Cannot be null or empty");
            if(string.IsNullOrEmpty(Password)) throw new SettingValidationException(nameof(ConnectionConfiguration), nameof(Password), "Cannot be null or empty");
            if(string.IsNullOrEmpty(VirtualHost)) throw new SettingValidationException(nameof(ConnectionConfiguration), nameof(VirtualHost), "Cannot be null or empty");
        }
    }
}