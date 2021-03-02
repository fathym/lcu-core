using Fathym;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
    [Serializable]
    [DataContract]
    public class LCUConfig : LCUVertex
    {
        [DataMember]
        public virtual string[] CapabilityFiles { get; set; }

        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual ModulePackSetup Modules { get; set; }

        [DataMember]
        public virtual MetadataModel Package { get; set; }

        [DataMember]
        public virtual DAFApplicationPackageTypes PackageType { get; set; }

        [DataMember]
        public virtual string RegScripts { get; set; }

        //[Obsolete]
        //[DataMember]
        //public virtual string NPMPackage { get; set; }

        //[Obsolete]
        //[DataMember]
        //public virtual string PackageVersion { get; set; }

        [DataMember]
        public virtual IdeSettingsConfigSolution[] Solutions { get; set; }

        [DataMember]
        public virtual MetadataModel StateConfig { get; set; }
    }
}
