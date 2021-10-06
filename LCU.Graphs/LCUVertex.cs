using System;

namespace LCU.Graphs
{
    public class LCUVertex
    {
        public virtual Guid ID { get; set; }

        public virtual string Label { get; set; }

        public virtual string Registry { get; set; }

        public virtual string TenantLookup { get; set; }
    }
}
