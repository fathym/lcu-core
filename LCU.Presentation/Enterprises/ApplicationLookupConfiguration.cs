using System;

namespace LCU.Presentation.Enterprises
{
	[Serializable]
	public class ApplicationLookupConfiguration
	{
		public virtual bool IsPrivate{ get; set; }

		public virtual bool IsReadOnly { get; set; }

		public virtual string PathRegex { get; set; }

		public virtual string QueryRegex { get; set; }

		public virtual string UserAgentRegex { get; set; }
	}
}
