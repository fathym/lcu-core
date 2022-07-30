
namespace Fathym.LCU.Hosting.Options
{
    public class LCUStartupAzureDataProtectionOptions
    {
        public virtual string BlobName { get; set; }

        public virtual string ConnectionString { get; set; }

        public virtual string ContainerName { get; set; }

        public virtual string KeyIdentifier { get; set; }
    }
}
