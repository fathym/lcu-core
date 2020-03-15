using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEPanel
	{
		[DataMember]
		public virtual string Panel { get; set; }

		[DataMember]
		public virtual string Title { get; set; }

		[DataMember]
		public virtual string Toolkit { get; set; }
	}
}
