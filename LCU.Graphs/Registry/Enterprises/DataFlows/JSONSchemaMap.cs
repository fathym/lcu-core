using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;
using System.Text;
using Fathym;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class JSONSchemaMap : BusinessModel<Guid>
    {
        [DataMember]
        public virtual bool Active { get; set; }

        [DataMember]
        public virtual string Description { get; set; }

        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual MetadataModel Schema { get; set; }
    }
}
