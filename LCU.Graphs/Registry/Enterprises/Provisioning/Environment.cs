using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	[Serializable]
	[DataContract]
	public class Environment : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string EnterprisePrimaryAPIKey { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}
}
