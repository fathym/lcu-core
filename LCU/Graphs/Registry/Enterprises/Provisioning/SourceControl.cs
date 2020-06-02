﻿using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	[Serializable]
	[DataContract]
	public class SourceControl : LCUVertex
	{
		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string Organization { get; set; }

		[DataMember]
		public virtual string Repository { get; set; }
	}
}
