using ExRam.Gremlinq.Core.GraphElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Graphs
{
	public class LCUEdge : IEdge
	{
		public virtual object Id { get; set; }

		public virtual string Label { get; set; }
	}
}
