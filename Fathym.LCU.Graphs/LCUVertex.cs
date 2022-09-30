using System;
using System.Runtime.Serialization;

namespace Fathym.LCU.Graphs
{
    [DataContract]
    public class LCUVertex
    {
        [DataMember]
        public virtual bool Archived { get; set; }

        [DataMember]
        public virtual DateTime Created { get; set; }

        [DataMember]
        public virtual Guid ID { get; set; }

        [DataMember]
        public virtual string Label { get; set; }

        [DataMember]
        public virtual string Registry { get; set; }

        [DataMember]
        public virtual string TenantLookup { get; set; }

        [DataMember]
        public virtual DateTime Updated { get; set; }
    }
}
