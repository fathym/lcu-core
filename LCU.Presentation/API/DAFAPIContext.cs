using System;
using System.Collections.Generic;

namespace LCU.Presentation.API
{
	[Serializable]
	public class DAFAPIsContext
	{
		public const string Lookup = "<DAF:APIs>";

		public virtual List<DAFAPIContext> APIs { get; set; }
	}

	[Serializable]
	public class DAFAPIContext
	{
		public virtual string APIRoot { get; set; }

		public virtual string InboundPath { get; set; }

		public virtual List<string> Methods { get; set; }

		public virtual string Security { get; set; }
	}
}
