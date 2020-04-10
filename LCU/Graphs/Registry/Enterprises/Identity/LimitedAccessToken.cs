using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
    [Serializable]
    [DataContract]
    public class LimitedAccessToken : BusinessModel<Guid>
    {
        [DataMember]
        public DateTime AccessStartDate { get; set; }

        [DataMember]
        public DateTime ExpirationDate { get; set; }

        [DataMember]
        public string EnterpriseAPIKey { get; set; }

        [DataMember]
        public bool IsLocked { get; set; }

        [DataMember]
        public string Registry { get; set; }

    }
}
