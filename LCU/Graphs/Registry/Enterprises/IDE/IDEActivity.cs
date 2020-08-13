using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEActivity : LCUVertex
	{
		[DataMember]
		public virtual string Icon { get; set; }

		[DataMember]
		public virtual string IconSet { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string Title { get; set; }
	}
}
