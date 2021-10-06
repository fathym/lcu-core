using Microsoft.OpenApi.Models;

namespace LCU.Hosting.Options
{
    public class LCUStartupAPISwaggerOptions
    {
        public virtual string Endpoint { get; set; }

        public virtual OpenApiInfo Info { get; set; }
    }
}
