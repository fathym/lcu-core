using Fathym;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class DAFAPIApplication : MetadataModel
	{
		[DataMember]
		public virtual string APIRoot { get; set; }

		[DataMember]
		public virtual string InboundPath { get; set; }

		[DataMember]
		public virtual string Methods { get; set; }

		[DataMember]
		public virtual string Security { get; set; }
	}
}
