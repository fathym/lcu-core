using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LCU.Testing
{
    public class GenericTests
    {
        #region Fields
        protected IConfiguration config;

        protected IConfigurationRoot hostConfig;
        #endregion

        #region Constructors
        public GenericTests()
        {
            setupHostConfiguration();

            setupConfiguration();
        }
        #endregion

        #region Helpers
        protected virtual ILogger<T> createLogger<T>()
        {
            return new LoggerFactory().CreateLogger<T>();
        }

        protected virtual DirectoryInfo getDirectory(string path)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var dirPath = Path.Combine(localAppData, path);

            return new DirectoryInfo(path);
        }

        protected virtual FileInfo getFile(string path)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var dirPath = Path.Combine(localAppData, path);

            return new FileInfo(path);
        }

        protected virtual void setupConfiguration()
        {
            var keyVaultName = hostConfig["LCU:Azure:KeyVault:Name"];

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("test.settings.json");

            if (!keyVaultName.IsNullOrEmpty())
            {
                var secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), 
                    new DefaultAzureCredential(includeInteractiveCredentials: false));

                configBuilder = configBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }
            else
            {
                configBuilder = configBuilder.AddUserSecrets(GetType().Assembly);
            }

            config = configBuilder
                .AddEnvironmentVariables()
                .Build();
        }

        protected virtual void setupHostConfiguration()
        {
            hostConfig = new ConfigurationBuilder()
               .AddJsonFile("host.settings.json")
               .Build();
        }
        #endregion
    }
}
