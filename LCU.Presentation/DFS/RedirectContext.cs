using System;

namespace LCU.Presentation.DFS
{
    [Serializable]
    public class RedirectContext
    {
        #region Constants
        public const string Lookup = "<DAF:Redirect>";
        #endregion

        public virtual bool Permanent { get; set; }

        public virtual bool PreserveMethod { get; set; }

        public virtual string Redirect { get; set; }
    }
}
