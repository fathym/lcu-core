using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration(configureAppConfig)
                        .UseStartup<Startup>();
                });

        private static void configureAppConfig(IConfigurationBuilder configBuilder)
        {
            var keyVaultName = "fathym-cloud-prd";

            if (!keyVaultName.IsNullOrEmpty())
            {
                var secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential(includeInteractiveCredentials: false));

                configBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }
        }
    }
}
