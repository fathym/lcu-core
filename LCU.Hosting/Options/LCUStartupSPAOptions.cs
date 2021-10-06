namespace LCU.Hosting.Options
{
    public class LCUStartupSPAOptions
    {
        public virtual string AppPath { get; set; }

        public virtual string ClientSourcePath { get; set; }

        public virtual LCUStartupSpaClientTypes ClientType { get; set; }

        public virtual bool IsPrivate { get; set; }

        public virtual string ServerURL { get; set; }
    }
}
