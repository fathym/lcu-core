﻿using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[Serializable]
	[DataContract]
	public class DataFlow : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual List<string> ModulePacks { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual DataFlowOutput Output { get; set; }
	}
}
