using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[Serializable]
	[DataContract]
	public class ModuleAction
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual bool Disabled { get; set; }

		[DataMember]
		public virtual ApplicationProfile Icon { get; set; }

		[DataMember]
		public virtual int Order { get; set; }

		[DataMember]
		public virtual string Text { get; set; }
	}
}
