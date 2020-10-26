using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public class DAFApplicationGitPackage
    {
        [DataMember]
        public virtual string Branch { get; set; }

        [DataMember]
        public virtual string Repository { get; set; }
    }
}
