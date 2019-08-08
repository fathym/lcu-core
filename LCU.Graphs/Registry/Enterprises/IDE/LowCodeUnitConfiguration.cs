using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class LowCodeUnitConfiguration
	{
		[DataMember]
		public virtual List<string> Files { get; set; }

		[DataMember]
		public virtual List<IdeSettingsConfigSolution> Solutions { get; set; }
	}
}
