using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public class DataFlowOutput : BusinessModel<Guid>
	{
		[DataMember]
		public virtual List<Module> Modules { get; set; }

		[DataMember]
		public virtual List<ModuleStream> Streams { get; set; }
	}
}
