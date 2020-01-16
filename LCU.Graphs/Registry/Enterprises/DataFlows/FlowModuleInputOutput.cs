using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class FlowModuleInputOutput
    {
        [DataMember]
        public virtual string ID { get; set; }

        [DataMember]
        public virtual ModuleControlType GateType { get; set; }

        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual string Settings { get; set; }

        [DataMember]
        public virtual string Type { get; set; }
    }
}