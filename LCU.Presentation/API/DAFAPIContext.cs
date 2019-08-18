using System;
using System.Collections.Generic;

namespace LCU.Presentation.API
{
	[Serializable]
	public class DAFAPIContext
	{
		public const string Lookup = "<DAF:API>";

		public virtual string APIRoot { get; set; }

		public virtual string InboundPath { get; set; }

		public virtual List<string> Methods { get; set; }

		public virtual string Security { get; set; }
	}
}
