using System;
using System.Runtime.Serialization;

namespace Fathym.LCU.Graphs
{
    [DataContract]
    public class LCUEdge
    {

        [DataMember]
        public virtual Guid ID { get; set; }

        [DataMember]
        public virtual Guid InV { get; set; }

        [DataMember]
        public virtual string Label { get; set; }

        [DataMember]
        public virtual Guid OutV { get; set; }

        [DataMember]
        public virtual string TenantLookup { get; set; }
    }
}
