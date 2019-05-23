using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.State
{
	[Serializable]
	[DataContract]
	public class LCUState
	{
		[DataMember]
		public virtual LCUStateConfiguration ActiveState { get; set; }

		[DataMember]
		public virtual bool? IsStateSettings { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }

		[DataMember]
		public virtual List<string> States { get; set; }
	}
}
