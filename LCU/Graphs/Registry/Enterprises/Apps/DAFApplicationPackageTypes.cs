using System;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public enum DAFApplicationPackageTypes
    {
        [EnumMember]
        Git,

        [EnumMember]
        NPM,

        [EnumMember]
        Zip
    }
}
