using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises
{
	[Serializable]
	[DataContract]
	public class Enterprise : LCUVertex
	{
		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual List<string> Hosts { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual bool PreventDefaultApplications { get; set; }

		[DataMember]
		public virtual string PrimaryAPIKey { get; set; }
	}
}
