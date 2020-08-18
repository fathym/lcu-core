using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	[Serializable]
	[DataContract]
	public class LCUEnvironment : LCUVertex
	{
		[DataMember]
		public virtual string Lookup { get; set; }
	}
}
