using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging.AzureAppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Hosting
{
    /// <summary>
    ///     Class used for constructing the standard LCU Host for web apps
    /// </summary>
    /// <example>
    ///     using LCU.Runtime.Hosting;
    ///     using System.Threading.Tasks;

    ///     namespace LCU.Runtime.Web
    ///     {
    ///         public class Program
    ///         {
    ///             public static async Task Main(string[] args)
    ///             {
    ///                 await LCUHostBuilder<Startup>.Start(args);
    ///             }
    ///         }
    ///     }
    /// </example>
    /// <typeparam name="TStartup">The type of the class to use for Startup.</typeparam>
    public class LCUHostBuilder<TStartup>
        where TStartup : class
    {
        #region Fields
        protected readonly IConfigurationRoot hostConfig;

        protected IHostBuilder innerHostBuilder;

        protected readonly ILogger logger;
        #endregion

        #region Constructors
        public LCUHostBuilder(string[] args)
            : this(Host.CreateDefaultBuilder(args ?? Array.Empty<string>()))
        { }

        public LCUHostBuilder(IHostBuilder innerHostBuilder)
        {
            if (innerHostBuilder == null)
                throw new ArgumentNullException("innerHostBuilder");

            this.innerHostBuilder = innerHostBuilder;

            hostConfig = setupHostConfiguration();

            logger = setupHostLogger();
        }
        #endregion

        #region Static
        public static async Task StartWebHost(string[] args)
        {
            await StartWebHost(Host.CreateDefaultBuilder(args));
        }

        public static async Task StartWebHost(IHostBuilder innerHostBuilder)
        {
            var hostBldr = new LCUHostBuilder<TStartup>(innerHostBuilder);

            hostBldr.ConfigureWebHost();

            await hostBldr.Build();
        }
        #endregion

        #region API Methods
        public virtual async Task Build()
        {
            logger.LogInformation($"Building host for {GetType().FullName}");

            var host = innerHostBuilder.Build();

            //var logger = loadLoggerFromHost(host);

            logger.LogInformation($"Built host for {GetType().FullName}, running now");

            try
            {
                await host.RunAsync();

                logger.LogInformation($"Ran host for {GetType().FullName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while running host for {GetType().FullName}");

                throw;
            }
        }

        public virtual void ConfigureWebHost()
        {
            logger.LogInformation($"Configuring web host for {GetType().FullName}");

            innerHostBuilder = innerHostBuilder.ConfigureWebHostDefaults(configureWebHost)
                .ConfigureAppConfiguration((hostCtxt, builder) =>
                {
                    if (!hostCtxt.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<TStartup>();
                    }
                });

            logger.LogInformation($"Configured host for {GetType().FullName}");
        }
        #endregion

        #region Helpers
        protected virtual void configureAppConfig(IConfigurationBuilder configBuilder)
        {
            var keyVaultName = hostConfig["LCU:Azure:KeyVault:Name"];

            if (!keyVaultName.IsNullOrEmpty())
            {
                var secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential(includeInteractiveCredentials: false));

                configBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            }
        }

        protected virtual void configureLogging(ILoggingBuilder logBuilder)
        {
            logger?.LogInformation("Configuring logging");

            logBuilder.AddApplicationInsights(hostConfig["LCU:Azure:ApplicationInsights:InstrumentationKey"]);

            logBuilder.AddAzureWebAppDiagnostics();

            logBuilder.AddConsole();

            //  TODO:  How to conditionally add this when hosted on windows
            //logBuilder.AddEventLog();

            logBuilder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(LCUHostBuilder<TStartup>).FullName,
                LogLevel.Trace);

            logger?.LogInformation("Configured logging");
        }

        protected virtual void configureServices(IServiceCollection services)
        {
            logger?.LogInformation("Configuring services");

            services.Configure<AzureFileLoggerOptions>(hostConfig.GetSection("Azure:FileLogging"));

            services.Configure<AzureBlobLoggerOptions>(hostConfig.GetSection("Azure:BlobLogging"));

            logger?.LogInformation("Configured services");
        }

        protected virtual void configureWebHost(IWebHostBuilder webBuilder)
        {
            logger?.LogInformation("Configuring web host");

            webBuilder
                .ConfigureAppConfiguration(configureAppConfig)
                .ConfigureLogging(configureLogging)
                .ConfigureServices(configureServices)
                .UseStartup<TStartup>();

            logger?.LogInformation("Configured web host");
        }

        //protected virtual ILogger<LCUHostBuilder<TStartup>> loadLoggerFromHost(IHost host)
        //{
        //    return (ILogger<LCUHostBuilder<TStartup>>)host.Services.GetService(typeof(ILogger<LCUHostBuilder<TStartup>>));
        //}

        protected virtual IConfigurationRoot setupHostConfiguration()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
               .AddJsonFile("host.settings.json")
               .AddJsonFile($"host.settings.{envName}.json", optional: true)
               .Build();
        }

        protected virtual ILogger setupHostLogger()
        {
            var loggerFactory = LoggerFactory.Create(configureLogging);

            return loggerFactory.CreateLogger(GetType());
        }
        #endregion
    }
}
