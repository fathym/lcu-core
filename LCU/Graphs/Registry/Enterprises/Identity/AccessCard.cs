using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[DataContract]
	public class AccessCard : LCUVertex  //BusinessModel<Guid>
	{
		[DataMember]
		public virtual string AccessConfigurationType { get; set; }

		[DataMember]
		public virtual string[] ExcludeAccessRights { get; set; }

		[DataMember]
		public virtual Audit FirstAccess { get; set; }

		[DataMember]
		public virtual string[] IncludeAccessRights { get; set; }

		[DataMember]
		public virtual Audit LastAccess { get; set; }

		[DataMember]
		public virtual string ProviderID { get; set; }

		[DataMember]
		public virtual DateTime ValidEndDate { get; set; }

		[DataMember]
		public virtual DateTime ValidStartDate { get; set; }
	}
}
