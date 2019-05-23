using System;

namespace LCU.Presentation.DFS
{
	[Serializable]
	public class DFSContext
	{
		public const string Lookup = "<DAF:DFS>";

		public virtual Guid ApplicationID { get; set; }

		public virtual string AppRoot { get; set; }

		public virtual int CacheSeconds { get; set; }

		public virtual string DefaultFile { get; set; }

		public virtual string DFSRoot { get; set; }

		public virtual Guid EnterpriseID { get; set; }
	}
}
