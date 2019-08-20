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
	public class Profile : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual Guid RelyingPartyID { get; set; }

		[DataMember]
		public virtual string Type { get; set; }
	}
}
