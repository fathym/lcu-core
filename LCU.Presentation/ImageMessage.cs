using System;
using System.Runtime.Serialization;

namespace LCU.Presentation
{
	[Serializable]
	[DataContract]
	public class ImageMessage
	{
		[DataMember]
		public virtual byte[] Data { get; set; }

        [DataMember]
        public virtual string DataString { get; set; }

        [DataMember]
		public virtual string Headers { get; set; }
	}
}
