using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using System.Collections.Generic;

namespace LCU.Hosting.Options
{
    public class LCUStartupEnterprisePipelineOptions
    {
        public virtual bool AllowBoot { get; set; }

        public virtual Dictionary<string, List<string>> CORSPoliciesOrigins { get; set; }

        public virtual LCUStartupAzureDataProtectionOptions DataProtection { get; set; }

        public virtual LCUStartupGitHubOptions GitHub { get; set; }

        public virtual string HostOverride { get; set; }

        public virtual LCUStartupHTTPClientOptions PersonasClients { get; set; }

        public virtual LCUStartupIdentityOptions Identity { get; set; }

        public virtual LCUStartupHTTPClientOptions ManagementClients { get; set; }

        public virtual List<string> PropagatedHeaders { get; set; }

        public virtual List<LCUResponseOptimizationTypes> ResponseOptimizations { get; set; }

        public virtual LCUStartupSPAOptions SPA { get; set; }

        public virtual bool UseHSTS { get; set; }
    }
}
