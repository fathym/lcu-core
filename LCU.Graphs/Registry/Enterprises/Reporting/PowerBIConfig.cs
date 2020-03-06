using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Reporting
{
	[Serializable]
	[DataContract]
	public class PowerBIConfig : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string APIUrl { get; set; }

		[DataMember]
		public virtual string AuthorityUrl { get; set; }

		[DataMember]
		public virtual string ClientId { get; set; }

		[DataMember]
		public virtual string GroupId { get; set; }

		[DataMember]
		public virtual string PrimaryAPIKey { get; set; }

		[DataMember]
		public virtual string Password { get; set; }

		[DataMember]
		public virtual string ResourceUrl { get; set; }

		[DataMember]
		public virtual string Username { get; set; }
	}
}
