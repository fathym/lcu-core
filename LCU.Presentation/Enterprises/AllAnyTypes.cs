using System.Runtime.Serialization;

namespace LCU.Presentation.Enterprises
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
