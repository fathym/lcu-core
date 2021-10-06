
namespace LCU.Hosting.Options
{
    public class LCUStartupOptions
    {
        #region Constants
        public const string ConfigKey = "LCU:Startup";
        #endregion

        public virtual LCUStartupEnterprisePipelineOptions Enterprise { get; set; }

        public virtual LCUStartupGlobalPipelineOptions Global { get; set; }
    }
}
