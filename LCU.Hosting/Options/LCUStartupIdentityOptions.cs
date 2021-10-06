using Microsoft.Identity.Web;
using System.Collections.Generic;

namespace LCU.Hosting.Options
{
    public class LCUStartupIdentityOptions
    {
        public virtual List<string> CustomPolicies { get; set; }

        public virtual MicrosoftIdentityOptions OpenID { get; set; }

        public virtual LCUStartupSessionsOptions Sessions { get; set; }
    }
}
