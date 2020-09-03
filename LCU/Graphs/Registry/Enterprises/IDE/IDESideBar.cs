using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDESideBar
	{
		[DataMember]
		public virtual List<SectionAction> Actions { get; set; }

		[DataMember]
		public virtual SectionAction CurrentAction { get; set; }

		[DataMember]
		public virtual string Title { get; set; }
	}

	[Serializable]
	[DataContract]
	public class SectionAction : LCUVertex
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual string Group { get; set; }

		[DataMember]
		public virtual string Section { get; set; }

		[DataMember]
		public virtual string Title { get; set; }
	}
}
