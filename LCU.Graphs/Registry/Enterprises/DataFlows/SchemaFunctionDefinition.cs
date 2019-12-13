using System;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    [DataContract]
    public class SchemaFunctionDefinition : BusinessModel<Guid>
    {
        [DataMember]
        public virtual bool Active { get; set; }

        [DataMember]
        public virtual string[] AllowedIncommingTypes { get; set; }

        [DataMember]
        public virtual bool AllowDifferentIncommingTypes { get; set; }

        [DataMember]
        public virtual bool AllowMultipleIncomming { get; set; }

        [DataMember]
        public virtual string Description { get; set; }

        [DataMember]
        public virtual string FunctionType { get; set; }

        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual int MaxProperties { get; set; }

        [DataMember]
        public virtual int MinProperties { get; set; }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual string ReturnType { get; set; }

        [DataMember]
        public virtual string SQL { get; set; }

        [DataMember]
        public virtual string SQLBoolean { get; set; }
    }
}