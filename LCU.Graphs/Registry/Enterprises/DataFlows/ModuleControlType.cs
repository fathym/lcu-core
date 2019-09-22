using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public enum ModuleControlType
	{
		[EnumMember]
		Direct,

		[EnumMember]
		Flow,

		[EnumMember]
		Gate,

		[EnumMember]
		Helper
	}
}
