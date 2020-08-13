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
		public virtual List<AccessRight> AccessRights { get; set; }

		[DataMember]
		public virtual List<AccessConfiguration> AccessConfigurations { get; set; }

		[DataMember]
		public virtual string DefaultAccessConfigurationType { get; set; }

		[DataMember]
		public virtual List<Provider> Providers { get; set; }
	}
}
