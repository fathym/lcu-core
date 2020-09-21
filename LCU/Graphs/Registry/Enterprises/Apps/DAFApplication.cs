using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class DAFApplication : LCUVertex
	{
		[DataMember]
		public virtual string ApplicationID { get; set; }

		[DataMember]
		public virtual MetadataModel Details { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }
    }
}
