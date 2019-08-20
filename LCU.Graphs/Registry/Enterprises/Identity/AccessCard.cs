using Fathym;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[DataContract]
	public class AccessCard : MetadataModel
	{
		[DataMember]
		public virtual string AccessConfigurationType { get; set; }

		[DataMember]
		public virtual List<Guid> ExcludeAccessRightIDs { get; set; }

		[DataMember]
		public virtual Audit FirstAccess { get; set; }

		[DataMember]
		public virtual List<Guid> IncludeAccessRightIDs { get; set; }

		[DataMember]
		public virtual Audit LastAccess { get; set; }

		[DataMember]
		public virtual Guid ProviderID { get; set; }

		[DataMember]
		public virtual DateTime ValidEndDate { get; set; }

		[DataMember]
		public virtual DateTime ValidStartDate { get; set; }
	}
}
