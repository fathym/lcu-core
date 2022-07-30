
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace Fathym.LCU.Hosting.Options
{
    public class LCUStartupGlobalPipelineOptions
    {
        public virtual LCUStartupAPIOptions API { get; set; }

        public virtual ApplicationInsightsServiceOptions ApplicationInsights { get; set; }

        public virtual LCUStartupCachingOptions Caching { get; set; }

        public virtual LCUStartupDebugOptions Debug { get; set; }

        public virtual LCUStartupExceptionOptions Exceptions { get; set; }

        public virtual LCUStartupURLRewriterOptions URLRewriter { get; set; }

        public virtual bool UseForwardedHeaders { get; set; }

        public virtual bool UseMockHost { get; set; }
    }
}
