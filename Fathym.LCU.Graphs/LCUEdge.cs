using System;

namespace Fathym.LCU.Graphs
{    
    public class LCUEdge
    {        
        public virtual Guid ID { get; set; }
        
        public virtual Guid InV { get; set; }

        public virtual string Label { get; set; }

        public virtual Guid OutV { get; set; }

        public virtual string TenantLookup { get; set; }
    }
}
