using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	[Serializable]
	[DataContract]
	public class EnvironmentSettings : LCUVertex
	{
		[DataMember]
		public virtual MetadataModel Settings { get; set; }





        // Migration properties
        [Obsolete]
        [DataMember]
        public virtual string AzureTenantID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureSubID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureAppID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureAppAuthKey { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureRegion { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureLocation { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string EnvironmentLookup { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string OrganizationLookup { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureDevOpsProjectID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string InfrastructureRepoName { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureFeedID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string AzureInfrastructureServiceEndpointID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string EnvironmentInfrastructureTemplate { get; set; }
    }
}
