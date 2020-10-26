using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public class DAFApplicationNPMPackage
    {
        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual string Version { get; set; }
    }
}
