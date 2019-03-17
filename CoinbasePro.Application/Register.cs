using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application
{
    public class Register
    {
        public static void Services(IServiceCollection services)
        {
            services.AddSingleton<TA4N.LogWrapper, TA4N.LogWrapper>();
        }
    }
}
