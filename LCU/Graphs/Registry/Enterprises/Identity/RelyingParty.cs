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
	public class RelyingParty : LCUVertex //BusinessModel<Guid>
	{
		[DataMember]
		public virtual AccessRight[] AccessRights { get; set; }

		[DataMember]
		public virtual AccessConfiguration[] AccessConfigurations { get; set; }

		[DataMember]
		public virtual string DefaultAccessConfigurationType { get; set; }

		[DataMember]
		public virtual Provider[] Providers { get; set; }
	}
}
