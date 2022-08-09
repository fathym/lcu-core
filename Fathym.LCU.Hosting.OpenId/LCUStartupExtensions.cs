using Fathym.Design;
using Fathym.LCU.Hosting;
using Fathym.LCU.Hosting.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder
{
    public static class LCURuntimeStartupPipelineExtensions
    {
        #region API Methods
        public static IApplicationBuilder UseLCUAPIPipeline(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger,
            LCUStartupGlobalPipelineOptions globalOpts)
        {
            logger.LogInformation($"Configuring LCU global pipeline setup");

            if (globalOpts != null)
            {
                logger.LogInformation($"using Fathym.LCU global pipeline setup");

                app.UseLCUExceptions(env, logger, globalOpts.Exceptions);

                app.UseLCUDebug(logger, globalOpts.Debug);

                app.UseLCUCaching(env, logger, globalOpts.Caching);

                app.UseLCUForwardedHeaders(logger, globalOpts.UseForwardedHeaders);

                app.UseLCUURLRewriter(logger, globalOpts.URLRewriter);

                app.UseLCUAPI(logger, globalOpts.API);

                app.UseRouting();

                app.UseLCUAPIEndpoints(logger, globalOpts.API);
            }

            return app;
        }

        #region Use Helpers
        public static IEndpointConventionBuilder MapLCUAPI(this IEndpointRouteBuilder endpoints, ILogger logger, LCUStartupAPIOptions apiOpts)
        {
            logger.LogInformation($"Configuring API endpoint");

            if (apiOpts != null)
            {
                logger.LogInformation($"Using API endpoint");

                return endpoints.MapControllers();
            }
            else
                return null;
        }

        public static void UseLCUAPI(this IApplicationBuilder app, ILogger logger, LCUStartupAPIOptions apiOpts)
        {
            logger.LogInformation($"Configuring API");

            if (apiOpts != null)
            {
                logger.LogInformation($"Using API");

                if (apiOpts.Swagger != null)
                {
                    logger.LogInformation($"Using swagger");

                    app.UseSwagger();

                    app.UseSwaggerUI(c => c.SwaggerEndpoint(apiOpts.Swagger.Endpoint, apiOpts.Name));
                }
            }
        }

        public static void UseLCUAPIEndpoints(this IApplicationBuilder app, ILogger logger, LCUStartupAPIOptions apiOpts)
        {
            logger.LogInformation($"Configuring LCU API endpoints");

            app.UseEndpoints(endpoints =>
            {
                if (apiOpts != null)
                {
                    logger.LogInformation($"using Fathym.LCU API endpoint");

                    endpoints.MapLCUAPI(logger, apiOpts);
                }
            });

        }

        public static void UseLCUCaching(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger,
            LCUStartupCachingOptions cacheOpts)
        {
            logger.LogInformation($"Configuring caching");

            if (cacheOpts != null)
            {
                logger.LogInformation($"Using caching");

                if (cacheOpts.Redis == null)
                    logger.LogInformation($"Using in memory distributed cache");
                else
                    logger.LogInformation($"Using redis distributed cache");
            }
        }

        public static void UseLCUDebug(this IApplicationBuilder app, ILogger logger, LCUStartupDebugOptions debugOpts)
        {
            logger.LogInformation($"Configuring debug");

            if (debugOpts != null)
            {
                logger.LogInformation($"Using debug middleware");

                app.UseMiddleware<RequestDebugMiddleware>();
            }
        }

        public static void UseLCUExceptions(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger,
            LCUStartupExceptionOptions exOpts)
        {
            logger.LogInformation($"Configuring exceptions");

            //if (exOpts == null || exOpts.HandlerPath.IsNullOrEmpty() || env.IsDevelopment())
            //{
            //    logger.LogInformation($"Using developer exception page");

            app.UseDeveloperExceptionPage();
            //}
            //else if (!exOpts.HandlerPath.IsNullOrEmpty())
            //{
            //    logger.LogInformation($"Using custom exception handler: {exOpts.HandlerPath}");

            //    app.UseExceptionHandler(exOpts.HandlerPath);
            //}
        }

        public static void UseLCUForwardedHeaders(this IApplicationBuilder app, ILogger logger, bool useForwardedHeaders)
        {
            logger.LogInformation($"Configuring forwarded headers");

            if (useForwardedHeaders)
            {
                logger.LogInformation($"Using forwarded headers");

                app.UseForwardedHeaders();
            }
        }

        public static void UseLCUURLRewriter(this IApplicationBuilder app, ILogger logger, LCUStartupURLRewriterOptions urlRewriteOpts)
        {
            logger.LogInformation($"Configuring URL rewriter");

            if (urlRewriteOpts != null)
            {
                logger.LogInformation($"Using URL rewriter");

                var options = new RewriteOptions();

                if (urlRewriteOpts.ForceHTTPs)
                {
                    logger.LogInformation($"Using forced HTTPs");

                    //options = options.AddRedirectToHttpsPermanent();

                    app.UseHttpsRedirection();
                }

                if (urlRewriteOpts.ForceWWW)
                {
                    logger.LogInformation($"Using forced WWW");

                    options = options.AddRedirectToWwwPermanent();

                    app.UseRewriter(options);
                }
            }
        }
        #endregion
        #endregion
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LCUStartupServiceExtensions
    {
        #region API Methods
        public static IServiceCollection AddLCUAPIPipeline(this IServiceCollection services,
            LCUStartupGlobalPipelineOptions globalOpts, IConfiguration config)
        {
            if (globalOpts != null)
            {
                services.AddLCUDebug(globalOpts.Debug);

                services.AddLCUExceptions(globalOpts.Exceptions);

                services.AddLCUCaching(globalOpts.Caching, config);

                services.AddLCUForwardedHeaders(globalOpts.UseForwardedHeaders);

                services.AddLCUAPI(globalOpts.API);

                services.AddLCUURLRewriter(globalOpts.URLRewriter);

                services.AddLogging();

                services.AddHttpContextAccessor();

                services.AddApplicationInsightsTelemetry(globalOpts.ApplicationInsights);
            }

            return services;
        }

        #region Add Helpers
        public static void AddLCUAPI(this IServiceCollection services, LCUStartupAPIOptions apiOpts)
        {
            if (apiOpts != null)
            {
                services.AddControllers()
                    .AddJsonOptions(o =>
                    {
                        DesignOutline.Instance.BuildCommonDefaultJSONSerialization(o.JsonSerializerOptions);

                        o.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                    });
                //.AddNewtonsoftJson(o =>
                //{
                //    if (!apiOpts.EnableCamelCasing)
                //        o.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //});

                if (apiOpts.Swagger != null)
                {
                    services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc(apiOpts.Swagger.Info.Version, apiOpts.Swagger.Info);
                    });

                    //services.AddSwaggerGenNewtonsoftSupport();
                }
            }
        }
        #endregion
        #endregion
    }
}
