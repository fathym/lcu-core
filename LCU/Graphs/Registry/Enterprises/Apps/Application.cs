using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class Application : BusinessModel<Guid>
	{
		[DataMember]
		public virtual List<string> AccessRights { get; set; }

		[DataMember]
		public virtual string Container { get; set; }

		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string EnterpriseAPIKey { get; set; }

		[DataMember]
		public virtual List<string> Hosts { get; set; }

		[DataMember]
		public virtual bool IsPrivate { get; set; }

		[DataMember]
		public virtual bool IsReadOnly { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string PathRegex { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }

		[DataMember]
		public virtual string QueryRegex { get; set; }

		[DataMember]
		public virtual string UserAgentRegex { get; set; }
	}
}
