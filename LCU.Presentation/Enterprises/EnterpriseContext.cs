using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Presentation.Enterprises
{
	[Serializable]
	public class EnterpriseContext
	{
		public const string Lookup = "<DAF:Enterprise>";

		public virtual int CacheSeconds { get; set; }

		public virtual string Host { get; set; }

		public virtual Guid ID { get; set; }

		public virtual string PrimaryAPIKey { get; set; }

		public virtual bool PreventDefaultApplications { get; set; }
	}
}
