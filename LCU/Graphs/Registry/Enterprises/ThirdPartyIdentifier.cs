using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises
{
	[DataContract]
	public class ThirdPartyIdentifier : LCUVertex
	{
		[DataMember]
		public virtual string Key { get; set; }

		[DataMember]
		public virtual string Value { get; set; }
	}
}
