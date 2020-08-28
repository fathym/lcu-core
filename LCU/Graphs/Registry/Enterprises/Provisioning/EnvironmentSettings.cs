using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	[Serializable]
	[DataContract]
	public class EnvironmentSettings : LCUVertex
	{
		[DataMember]
		public virtual MetadataModel Settings { get; set; }
	}
}
