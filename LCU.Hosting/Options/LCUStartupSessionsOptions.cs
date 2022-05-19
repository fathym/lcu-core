using Microsoft.AspNetCore.Builder;

namespace LCU.Hosting.Options
{
    public class LCUStartupSessionsOptions
    {
        public virtual string CookieName { get; set; }

        public virtual int IdleTimeoutMinutes { get; set; }
    }
}
