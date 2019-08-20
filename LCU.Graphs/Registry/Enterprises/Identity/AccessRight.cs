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
	public class AccessRight : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}
}
