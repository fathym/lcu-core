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
    public static class LCUHttpServiceExtensions
    {
        #region API Methods
        public static void AddConfig<T>(this IServiceCollection services, string key, IConfiguration config)
            where T : class, new()
        {
            var configSec = config.GetSection(key);

            services.Configure<T>(configSec);

            var cfg = new T();

            configSec.Bind(cfg);

            services.AddSingleton(cfg);
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
    }
}
