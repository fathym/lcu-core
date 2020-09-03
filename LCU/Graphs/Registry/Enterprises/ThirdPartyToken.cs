using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises
{
	[DataContract]
	public class ThirdPartyToken : LCUVertex
	{
		[DataMember]
		public virtual bool Encrypt { get; set; }

		[DataMember]
		public virtual string Key { get; set; }

		[DataMember]
		public virtual string Token { get; set; }
	}
}
