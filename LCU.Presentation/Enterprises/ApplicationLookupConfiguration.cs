using System;
using System.Collections.Generic;

namespace LCU.Presentation.Enterprises
{
	[Serializable]
	public class ApplicationLookupConfiguration
	{
		public virtual List<string> AccessRights { get; set; }

		public virtual bool IsPrivate{ get; set; }

		public virtual bool IsReadOnly { get; set; }

		public virtual List<string> Licenses { get; set; }

		public virtual string PathRegex { get; set; }

		public virtual string QueryRegex { get; set; }

		public virtual string UserAgentRegex { get; set; }
	}
}
