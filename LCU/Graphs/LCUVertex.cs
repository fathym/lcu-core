using ExRam.Gremlinq.Core.GraphElements;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Graphs
{
	public class LCUVertex : BusinessModel<Guid>
	{
		public virtual string Registry { get; set; }

		#region Constructors
		public LCUVertex()
		{
			Registry = "Registry";
		}
		#endregion
	}
}
