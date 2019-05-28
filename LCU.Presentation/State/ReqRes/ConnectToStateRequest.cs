using Fathym;
using Fathym.API;
using System;
using System.Runtime.Serialization;

namespace LCU.Presentation.State.ReqRes
{
	[Serializable]
	[DataContract]
	public class ConnectToStateRequest : BaseRequest
	{
		[DataMember]
		public virtual string Environment { get; set; }

		[DataMember]
		public virtual string Key { get; set; }

		[DataMember]
		public virtual bool? ShouldSend { get; set; }

		[DataMember]
		public virtual string State { get; set; }

		[DataMember]
		public virtual string UsernameMock { get; set; }
	}
}
