using CoinbasePro.Application.HostedServices.Gather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TA4N;

namespace CoinbasePro.Application.RabbitMq
{
    public static class Register
    {
        public static void AddRabbitMq(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IJsonUtil, JsonUtil>();
            services.AddTransient<ICandleConsumer, ClientConsumer>();
            services.AddTransient<ICandleProducer, ClientProducer>();

            services.Configure<ConnectionConfiguration>(config.GetSection("ConnectionConfiguration"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ConnectionConfiguration>>().Value);
            services.AddSingleton<IValidateStartUp>(resolver =>
                resolver.GetRequiredService<IOptions<ConnectionConfiguration>>().Value);

            services.Configure<ChannelConfiguration>(config.GetSection("ChannelConfiguration"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ChannelConfiguration>>().Value);
            services.AddSingleton<IValidateStartUp>(resolver =>
                resolver.GetRequiredService<IOptions<ChannelConfiguration>>().Value);
        }
    }
}
