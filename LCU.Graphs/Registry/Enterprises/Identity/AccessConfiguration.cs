using Fathym;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[DataContract]
	public class AccessConfiguration : MetadataModel
	{
		[DataMember]
		public virtual List<Guid> AcceptedProviderIDs { get; set; }

		[DataMember]
		public virtual List<Guid> AccessRightIDs { get; set; }

		[DataMember]
		public virtual string Type { get; set; }
	}
}
