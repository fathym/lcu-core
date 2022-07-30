using Fathym.LCU.Hosting.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Fathym.LCU.Hosting
{
    public class LCUAPIStartup : LCUStartup
    {
        #region Fields
        #endregion

        #region Constructors
        public LCUAPIStartup(IConfiguration config)
            : base(config)
        { }
        #endregion

        #region API Methods
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<LCUAPIStartup> logger,
            IOptions<LCUStartupOptions> startupOptions)
        {
            app.UseLCUAPIPipeline(env, logger, (LCUStartupGlobalPipelineOptions)startupOptions.Value.Global);
        }
        #endregion

        #region Helpers
        protected override void configureServices(IServiceCollection services, LCUStartupOptions startupOptions)
        {
            base.configureServices(services, startupOptions);

            services.AddLCUAPIPipeline(startupOptions?.Global, config);
        }
        #endregion
    }
}
