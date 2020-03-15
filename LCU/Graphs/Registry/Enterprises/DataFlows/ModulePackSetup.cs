using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[Serializable]
	[DataContract]
	public class ModulePackSetup
	{
		[DataMember]
		public virtual List<ModuleDisplay> Displays { get; set; }

		[DataMember]
		public virtual List<ModuleOption> Options { get; set; }

		[DataMember]
		public virtual ModulePack Pack { get; set; }
	}
}
