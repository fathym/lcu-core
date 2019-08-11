using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IdeSettingsSectionAction
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual string Group { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}
}
