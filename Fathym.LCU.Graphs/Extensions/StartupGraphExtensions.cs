using Fathym.LCU.Graphs;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupGraphExtensions
    {
        public static void AddLCUGraphConfig(this IServiceCollection services, IConfiguration config)
        {
            AddLCUGraphConfig(services, "LCU:Graph", config);
        }

        public static void AddLCUGraphConfig(this IServiceCollection services, string key,
            IConfiguration config)
        {
            var graphConfigSec = config.GetSection(key);

            services.Configure<LCUGraphConfig>(graphConfigSec);

            var graphConfig = new LCUGraphConfig();

            graphConfigSec.Bind(graphConfig);

            services.AddSingleton(graphConfig);
        }
    }
}
