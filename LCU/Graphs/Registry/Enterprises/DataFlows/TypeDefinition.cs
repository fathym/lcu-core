using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class TypeDefinition : BusinessModel<Guid>
    {
        [DataMember]
        public virtual bool Active { get; set; }

        [DataMember]
        public virtual IEnumerable<string> AllowedTypeNameConversions { get; set; }

        [DataMember]
        public virtual string ConversionMethod { get; set; }

        [DataMember]
        public virtual string Description { get; set; }

        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual string TypeName { get; set; }
    }
}