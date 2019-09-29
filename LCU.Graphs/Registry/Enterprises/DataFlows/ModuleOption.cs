using Fathym.Business.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[Serializable]
	[DataContract]
	public class ModuleOption : BusinessModel<Guid>
	{
		[DataMember]
		public virtual bool Active { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public virtual ModuleControlType ControlType { get; set; }

		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual int IncomingConnectionLimit { get; set; }

		[DataMember]
		public virtual IEnumerable<string> IncomingConnectionTypes { get; set; }

		[DataMember]
		public virtual string ModuleType { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual int OutgoingConnectionLimit { get; set; }

		[DataMember]
		public virtual IEnumerable<string> OutgoingConnectionTypes { get; set; }

		[DataMember]
		public virtual bool Visible { get; set; }
	}
}
