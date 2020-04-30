using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public enum ModuleShapeTypes
	{
		[EnumMember]
		Rectangle,

		[EnumMember]
		Circle,

		[EnumMember]
		Ellipse,

		[EnumMember]
		Custom
	}
}
