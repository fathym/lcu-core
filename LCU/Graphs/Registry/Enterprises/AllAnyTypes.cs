using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises
{
    [DataContract]
	public enum AllAnyTypes
	{
		[EnumMember]
		All,

		[EnumMember]
		Any
	}
}
