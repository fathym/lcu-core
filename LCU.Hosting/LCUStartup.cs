using LCU.Hosting.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace LCU.Hosting
{
    public class LCUStartup
    {
        #region Fields
        protected readonly IConfiguration config;
        #endregion

        #region Constructors
        public LCUStartup(IConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
        #endregion

        #region API Methods
        public virtual void ConfigureServices(IServiceCollection services)
        {
            var startupOptions = services.AddOptions<LCUStartupOptions>(config, LCUStartupOptions.ConfigKey);

            configureServices(services, startupOptions);
        }
        #endregion

        #region Helpers
        protected virtual void configureServices(IServiceCollection services, LCUStartupOptions startupOptions)
        { }
        #endregion
    }
}
