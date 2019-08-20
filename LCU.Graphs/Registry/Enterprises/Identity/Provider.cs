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
	public class Provider : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string Type { get; set; }

		[DataMember]
		public virtual bool LockoutEnabled { get; set; }
	}
}
