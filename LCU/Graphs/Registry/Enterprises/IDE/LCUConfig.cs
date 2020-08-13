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
		public virtual string NPMPackage { get; set; }

		[DataMember]
		public virtual string PackageVersion { get; set; }

		[DataMember]
		public virtual IdeSettingsConfigSolution[] Solutions { get; set; }
	}
}
