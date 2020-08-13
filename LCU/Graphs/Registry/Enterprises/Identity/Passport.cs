using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[Serializable]
	[DataContract]
	public class Passport : LCUVertex  //BusinessModel<Guid>
	{
		[DataMember]
        public virtual bool IsActive { get; set; }

		[DataMember]
		public virtual string PasswordHash { get; set; }

		[DataMember]
		public virtual string ProviderID { get; set; }
	}
}
