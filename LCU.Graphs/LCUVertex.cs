using System;

namespace LCU.Graphs
{
    public class LCUVertex
    {
        public virtual bool Archived { get; set; }

        public virtual DateTime Created { get; set; }

        public virtual Guid ID { get; set; }

        public virtual string Label { get; set; }

        public virtual string Registry { get; set; }

        public virtual string TenantLookup { get; set; }

        public virtual DateTime Updated { get; set; }
    }
}
