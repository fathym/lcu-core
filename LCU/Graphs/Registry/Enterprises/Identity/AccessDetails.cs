using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[DataContract]
	public class AccessDetails
	{
		[DataMember]
		public virtual int AccessFailedCount { get; set; }

		[DataMember]
		public virtual DateTime? FirstAccess { get; set; }

		[DataMember]
		public virtual DateTime? LastAccess { get; set; }

		[DataMember]
		public virtual bool LockoutEnabled { get; set; }

		[DataMember]
		public virtual DateTimeOffset? LockoutEnd { get; set; }

		[DataMember]
		public string ProviderAPIKey { get; set; }
	}
}
