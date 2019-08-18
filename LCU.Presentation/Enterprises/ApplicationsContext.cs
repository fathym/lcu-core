using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Presentation.Enterprises
{
	[Serializable]
	public class ApplicationsContext
	{
		public const string Lookup = "<DAF:Applications>";

		public virtual List<ApplicationContext> Applications { get; set; }

		public virtual string Container { get; set; }
	}
}
