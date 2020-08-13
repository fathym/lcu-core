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
		public virtual Module[] Modules { get; set; }

		[DataMember]
		public virtual ModuleStream[] Streams { get; set; }
	}
}
