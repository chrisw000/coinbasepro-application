using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;

namespace CoinbasePro.Application
{
    public static class Register
    {
        // in ASPNETCORE website this value is in the launchSettings.json file
        private const string keyVaultKey = "ASPNETCORE_HOSTINGSTARTUP__KEYVAULT__CONFIGURATIONVAULT";

        public static void AddApplication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

            services.AddSingleton<TA4N.LogWrapper, TA4N.LogWrapper>();
        }

        public static void AddAzureKeyVault(IConfigurationRoot builtConfig, IConfigurationBuilder configurationBuilder)
        {
            if (!string.IsNullOrEmpty(builtConfig[keyVaultKey]))
            {
                // To enable running locally follow steps here:
                // https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication
                // Install Azure CLI v2.0.57 at time of writing (06/02/2019)
                // from Azure CLI, run:
                // az login
                // az account get-access-token
                // ---> the token now allows the Azure part to work correctly

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                configurationBuilder.AddAzureKeyVault(
                    builtConfig[keyVaultKey],
                    keyVaultClient,
                    new DefaultKeyVaultSecretManager());
            }
        }
    }
}
