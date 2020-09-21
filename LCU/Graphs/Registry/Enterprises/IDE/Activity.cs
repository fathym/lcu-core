using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class Activity : LCUVertex
	{
		[DataMember]
		public virtual string Icon { get; set; }

		[DataMember]
		public virtual string IconSet { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string[] Sections { get; set; }

		[DataMember]
		public virtual string Title { get; set; }
	}
}
