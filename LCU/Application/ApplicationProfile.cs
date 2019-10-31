using Fathym.Business.Models;
using System;
using System.Runtime.Serialization;

namespace LCU
{
    [DataContract]
    public class ApplicationProfile
    {
        [DataMember]
        public virtual int DatabaseClientMaxPoolConnections { get; set; }

        [DataMember]
        public virtual int DatabaseClientPoolSize { get; set; }

        [DataMember]
        public virtual int DatabaseClientTTLMinutes { get; set; }
    }
}
