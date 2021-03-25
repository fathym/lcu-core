using Fathym;
using Fathym.Business.Models;
using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
	[DataContract]
	public class Application : LCUVertex
	{
		[DataMember]
		public virtual MetadataModel Config { get; set; }

		[DataMember]
		public virtual string Container { get; set; }

		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string[] Hosts { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }
		
		//[Obsolete]
		//[DataMember]
		//public virtual string[] AccessRights { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual bool IsPrivate { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual bool IsReadOnly { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual string[] Licenses { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual string PathRegex { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual string QueryRegex { get; set; }

		//[Obsolete]
		//[DataMember]
		//public virtual string UserAgentRegex { get; set; }
	}
}
