using Fathym.Business.Models;
using System;
using System.Collections.Generic;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[Serializable]
	public class Passport : BusinessModel<Guid>
	{
        public virtual string EnterpriseAPIKey { get; set; }

        public virtual bool IsActive { get; set; }

		public virtual string PasswordHash { get; set; }
	}
}
