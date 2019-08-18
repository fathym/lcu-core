using System;
using System.Collections.Generic;

namespace LCU.Presentation.Identity
{
	[Serializable]
	public class LCUAuthorizationContext
	{
		public const string Lookup = "<LCU:Authorization>";

		public virtual List<string> Schemes { get; set; }
	}
}