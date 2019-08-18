using Fathym;
using Fathym.API;
using System;
using System.Runtime.Serialization;

namespace LCU.Presentation.State.ReqRes
{
	[Serializable]
	[DataContract]
	public class ExecuteActionRequest : BaseRequest
	{
		[DataMember]
		public virtual MetadataModel Arguments { get; set; }

		[DataMember]
		public virtual string Key { get; set; }

		[DataMember]
		public virtual string State { get; set; }

		[DataMember]
		public virtual string Type { get; set; }
	}
}
