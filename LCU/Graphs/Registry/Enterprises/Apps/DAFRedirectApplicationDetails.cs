using Fathym;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class DAFRedirectApplicationDetails
	{
		[DataMember]
		public virtual string Redirect { get; set; }
	}
}
