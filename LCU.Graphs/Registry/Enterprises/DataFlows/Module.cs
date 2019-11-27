using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[Serializable]
	[DataContract]
	public class Module : BusinessModel<Guid>
	{
		[DataMember]
		public virtual bool Deleted { get; set; }

		[DataMember]
		public virtual ModuleDisplay Display { get; set; }

		[DataMember]
		public virtual MetadataModel Settings { get; set; }

		[DataMember]
		public virtual Status Status { get; set; }

		[DataMember]
		public virtual string Text { get; set; }

		[DataMember]
		public virtual string Toolkit { get; set; }
	}
}
