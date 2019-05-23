﻿using System.Collections.Generic;

namespace LCU.Graphs
{
	public class LCUGraphConfig
	{
		#region Properties
		public virtual string APIKey { get; set; }

		public virtual string Graph { get; set; }

		public virtual string Database { get; set; }

		public virtual bool EnableSSL { get; set; }

		public virtual string Host { get; set; }

		public virtual int Port { get; set; }
		#endregion

		#region Constructors
		public LCUGraphConfig()
		{
			EnableSSL = true;

			Port = 443;
		}
		#endregion
	}
}
