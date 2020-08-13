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
		public virtual List<string> ExcludeAccessRights { get; set; }

		[DataMember]
		public virtual Audit FirstAccess { get; set; }

		[DataMember]
		public virtual List<string> IncludeAccessRights { get; set; }

		[DataMember]
		public virtual Audit LastAccess { get; set; }

		[DataMember]
		public virtual Guid ProviderID { get; set; }

        [DataMember]
        public virtual string Registry { get; set; }

		[DataMember]
		public virtual DateTime ValidEndDate { get; set; }

		[DataMember]
		public virtual DateTime ValidStartDate { get; set; }
	}
}
