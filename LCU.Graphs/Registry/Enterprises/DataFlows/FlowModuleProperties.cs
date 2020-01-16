using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class FlowModuleProperties
    {
        [DataMember]
        public virtual string CTEQuery { get; set; }

        [DataMember]
        public virtual List<string> ErrorList { get; set; }

        [DataMember]
        public virtual IEnumerable<FlowModuleInputOutput> Inputs { get; set; }

        [DataMember]
        public virtual bool IsValid { get; set; }

        [DataMember]
        public virtual IEnumerable<FlowModuleInputOutput> Outputs { get; set; }

        [DataMember]
        public virtual string Query { get; set; }
    }
}

