using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class LowCodeUnitSetupConfig
	{
		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string NPMPackage { get; set; }

		[DataMember]
		public virtual string PackageVersion { get; set; }
	}
}
