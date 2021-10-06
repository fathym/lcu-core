
namespace LCU.Hosting.Options
{
    public class LCUStartupAPIOptions
    {
        public virtual bool EnableCamelCasing { get; set; }

        public virtual string Name { get; set; }

        public virtual LCUStartupAPISwaggerOptions Swagger { get; set; }
    }
}
