using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IdeSettingsConfigSolution
	{
		[DataMember]
		public virtual string Element { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}
}
