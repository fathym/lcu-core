using Fathym.API;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Presentation.State.ReqRes
{
	[Serializable]
	[DataContract]
	public class ReceiveStateRequest : BaseRequest
	{
		[DataMember]
		public virtual IDictionary<string, object> State { get; set; }
	}
}
