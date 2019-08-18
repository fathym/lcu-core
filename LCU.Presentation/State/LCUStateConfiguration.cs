using Fathym;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Presentation.State
{
	[Serializable]
	[DataContract]
	public class LCUStateConfiguration
	{
		[DataMember]
		public virtual IDictionary<string, LCUStateAction> Actions { get; set; }

		[DataMember]
		public virtual string ActiveEnvironment { get; set; }

		[DataMember]
		public virtual MetadataModel DefaultValue { get; set; }

		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual IDictionary<string, LCUStateEnvironment> Environments { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual bool UseUsername { get; set; }
	}

	[Serializable]
	[DataContract]
	public class LCUStateAction
	{
		[DataMember]
		public virtual string APIRoot { get; set; }

		[DataMember]
		public virtual string Security { get; set; }
	}

	[Serializable]
	[DataContract]
	public class LCUStateEnvironment
	{
		[DataMember]
		public virtual string Security { get; set; }

		[DataMember]
		public virtual string ServerAPIRoot { get; set; }
	}
}
