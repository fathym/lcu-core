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
		public virtual Guid[] AcceptedProviderIDs { get; set; }

		[DataMember]
		public virtual string[] AccessRights { get; set; }

		[DataMember]
		public virtual string Type { get; set; }
	}
}
