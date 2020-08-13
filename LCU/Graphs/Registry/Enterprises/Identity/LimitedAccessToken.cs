﻿using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
    [Serializable]
    [DataContract]
    public class LicenseAccessToken : LCUVertex  //BusinessModel<Guid>
    {
        [DataMember]
        public DateTime AccessStartDate { get; set; }

        [DataMember]
        public DateTime ExpirationDate { get; set; }

        [DataMember]
        public Fathym.Status EnterpriseOverride { get; set; }

        [DataMember]
        public bool IsLocked { get; set; }

        [DataMember]
        public bool IsReset { get; set; }

        [DataMember]
        public string Lookup { get; set; }

        [DataMember]
        public int TrialPeriodDays { get; set; }

        [DataMember]
        public string Username { get; set; }

    }
}
