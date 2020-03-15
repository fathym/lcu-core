using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class FlowTypeDependency
    {

        [DataMember]
        public virtual string ModuleID { get; set; }

        [DataMember]
        public virtual string ModuleName { get; set; }

        [DataMember]
        public virtual string MasterID { get; set; }

        [DataMember]
        public virtual string SchemaID { get; set; }

        [DataMember]
        public virtual string SchemaName { get; set; }

        [DataMember]
        public virtual string Type { get; set; }
    }
}

