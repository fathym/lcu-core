using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LCUConfigurationExtentsions
    {
        #region API Methods
        public static TConfig AddConfig<TConfig>(this IServiceCollection services, IConfiguration config, string configKey)
            where TConfig : class, new()
        {
            var boundConfig = new TConfig();

            config.Bind(configKey, boundConfig);

            services.AddSingleton(boundConfig);

            return boundConfig;
        }

        public static TOptions AddOptions<TOptions>(this IServiceCollection services, IConfiguration config, string configKey)
            where TOptions : class, new()
        {
            var optsCfg = config.GetSection(configKey);

            services.Configure<TOptions>(optsCfg);

            return optsCfg.Get<TOptions>();
        }

        public static TOptions GetOptions<TOptions>(this IConfiguration config, string configKey)
        {
            var optsCfg = config.GetSection(configKey);

            return optsCfg.Get<TOptions>();
        }
        #endregion
    }
}
