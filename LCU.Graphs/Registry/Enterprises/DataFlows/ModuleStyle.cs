using Fathym.Business.Models;
using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public class ModuleStyle : BusinessModel<Guid>
	{
		[DataMember]
		public virtual double Height { get; set; }

		[DataMember]
		public virtual string ModuleType { get; set; }

		[DataMember]
		public virtual ModuleShapeTypes Shape { get; set; }

		[DataMember]
		public virtual double Width { get; set; }
	}
}
