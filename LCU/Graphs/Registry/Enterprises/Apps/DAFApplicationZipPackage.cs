using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public class DAFApplicationZipPackage
    {
        [DataMember]
        public virtual string ZipFile { get; set; }
    }
}
