using Fathym.Design;
using LCU.Hosting;
using LCU.Hosting.Options;
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
        public static void AddConfig<T>(this IServiceCollection services, string key, IConfiguration config)
            where T : class, new()
        {
            var configSec = config.GetSection(key);

            services.Configure<T>(configSec);

            var cfg = new T();

            configSec.Bind(cfg);

            services.AddSingleton(cfg);
        }

        public static void AddLCUAPI(this IServiceCollection services, LCUStartupAPIOptions apiOpts)
        {
            if (apiOpts != null)
            {
                services.AddControllers()
                    .AddJsonOptions(o =>
                    {
                        DesignOutline.Instance.BuildCommonDefaultJSONSerialization(o);

                        if (apiOpts.EnableCamelCasing)
                            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
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

        public static void AddLCUCaching(this IServiceCollection services, LCUStartupCachingOptions cacheOpts, IConfiguration config)
        {
            if (cacheOpts != null)
            {
                services.AddMemoryCache();

                if (cacheOpts.Redis == null)
                    services.AddDistributedMemoryCache();
                else
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = config[cacheOpts.Redis.Configuration] ?? cacheOpts.Redis.Configuration;

                        options.InstanceName = cacheOpts.Redis.InstanceName;
                    });
            }
        }

        public static void AddLCUDebug(this IServiceCollection services, LCUStartupDebugOptions debugOpts)
        {
        }

        public static void AddLCUExceptions(this IServiceCollection services, LCUStartupExceptionOptions exOpts)
        {
        }

        public static void AddLCUForwardedHeaders(this IServiceCollection services, bool useForwardedHeaders)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;

                //  Todo: restrict to something ... 147.243.119.241
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public static IHttpClientBuilder AddLCUHTTPClient<TClient>(this IServiceCollection services,
            IPolicyRegistry<string> registry, LCUStartupHTTPClientOptions httpOpts)
            where TClient : class
        {
            var clientName = typeof(TClient).Name;

            var clientOptions = httpOpts.Options[clientName];

            return clientOptions.BaseAddress.IsNullOrEmpty() ? null : services.AddLCUHTTPClient<TClient>(registry, new Uri(clientOptions.BaseAddress));
        }

        public static IHttpClientBuilder AddLCUHTTPClient<TClient>(this IServiceCollection services,
            IPolicyRegistry<string> registry, Uri baseAddress, int retryCycles = 3, int retrySleepDurationMilliseconds = 500,
            int circuitFailuresAllowed = 5, int circuitBreakDurationSeconds = 5)
            where TClient : class
        {
            return services
                .AddRefitClient<TClient>(services =>
                {
                    return new RefitSettings()
                    {
                        //ContentSerializer = new NewtonsoftJsonContentSerializer()
                    };
                })
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = baseAddress;
                });
                //.AddLCUTimeoutPolicy(registry)
                //.AddTransientHttpErrorPolicy(p =>
                //{
                //    return p.WaitAndRetryAsync(retryCycles, _ =>
                //    {
                //        return TimeSpan.FromMilliseconds(retrySleepDurationMilliseconds);
                //    });
                //})
                //.AddTransientHttpErrorPolicy(p =>
                //{
                //    return p.CircuitBreakerAsync(circuitFailuresAllowed,
                //        TimeSpan.FromSeconds(circuitBreakDurationSeconds));
                //});
        }

        public static IPolicyRegistry<string> AddLCUPollyRegistry(this IServiceCollection services,
            LCUStartupHTTPClientOptions httpOpts)
        {
            var registry = services.AddPolicyRegistry();

            if (httpOpts != null)
            {
                var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(httpOpts.TimeoutSeconds));

                registry.Add("regular", timeout);

                var longTimeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(httpOpts.LongTimeoutSeconds));

                registry.Add("long", longTimeout);
            }

            return registry;
        }

        public static IHttpClientBuilder AddLCUTimeoutPolicy(this IHttpClientBuilder httpClientBuilder,
            IPolicyRegistry<string> registry)
        {
            return httpClientBuilder
                .AddPolicyHandler(request =>
                {
                    var timeoutPolicy = "regular";

                    if (request.Method != HttpMethod.Get)
                        timeoutPolicy = "long";

                    return registry.Get<IAsyncPolicy<HttpResponseMessage>>(timeoutPolicy);
                });
        }

        public static void AddLCUURLRewriter(this IServiceCollection services, LCUStartupURLRewriterOptions urlRewriteOpts)
        {
            if (urlRewriteOpts != null)
            {
            }
        }
        #endregion
        #endregion
    }
}

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
                logger.LogInformation($"Using LCU global pipeline setup");

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
                    logger.LogInformation($"Using LCU API endpoint");

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
