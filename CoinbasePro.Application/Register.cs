using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application
{
    public static class Register
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

            services.AddSingleton<TA4N.LogWrapper, TA4N.LogWrapper>();
        }
    }
}
