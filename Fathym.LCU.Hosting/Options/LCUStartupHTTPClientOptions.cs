using System.Collections.Generic;

namespace Fathym.LCU.Hosting.Options
{
    public class LCUStartupHTTPClientOptions
    {
        public virtual int CircuitBreakDurationSeconds { get; set; }

        public virtual int CircuitFailuresAllowed { get; set; }

        public virtual int LongTimeoutSeconds { get; set; }

        public virtual Dictionary<string, LCUClientOptions> Options { get; set; }

        public virtual int RetryCycles { get; set; }

        public virtual int RetrySleepDurationMilliseconds { get; set; }

        public virtual int TimeoutSeconds { get; set; }
    }
}
