using Microsoft.AspNetCore.Builder;

namespace Fathym.LCU.Hosting.Options
{
    public class LCUStartupSessionsOptions
    {
        public virtual string CookieName { get; set; }

        public virtual int IdleTimeoutMinutes { get; set; }
    }
}
