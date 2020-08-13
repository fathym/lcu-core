using Fathym.Business.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	[DataContract]
	public class ModuleDisplay : LCUVertex //BusinessModel<Guid>
	{
		[DataMember]
		public virtual ModuleAction[] Actions { get; set; }

		[DataMember]
		public virtual string Category { get; set; }

		[DataMember]
		public virtual string Element { get; set; }

		[DataMember]
		public virtual double Height { get; set; }

		[DataMember]
		public virtual LCUIcon Icon { get; set; }

		[DataMember]
		public virtual double Left { get; set; }

		[DataMember]
		public virtual string ModuleType { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public virtual ModuleShapeTypes Shape { get; set; }

		[DataMember]
		public virtual string Toolkit { get; set; }

		[DataMember]
		public virtual double Top { get; set; }

		[DataMember]
		public virtual double Width { get; set; }
	}
}
