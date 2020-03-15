using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEEditor
	{
		[DataMember]
		public virtual string Editor { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string Title { get; set; }

		[DataMember]
		public virtual string Toolkit { get; set; }
	}
}
