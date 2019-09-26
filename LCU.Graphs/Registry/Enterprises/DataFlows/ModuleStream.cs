using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public class ModuleStream : BusinessModel<Guid>
	{
		[DataMember]
		public virtual Guid InputModuleID { get; set; }

		[DataMember]
		public virtual Guid OutputModuleID { get; set; }

		[DataMember]
		public virtual string Title { get; set; }
	}
}
