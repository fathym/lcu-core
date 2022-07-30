using Microsoft.OpenApi.Models;

namespace Fathym.LCU.Hosting.Options
{
    public class LCUStartupAPISwaggerOptions
    {
        public virtual string Endpoint { get; set; }

        public virtual OpenApiInfo Info { get; set; }
    }
}
