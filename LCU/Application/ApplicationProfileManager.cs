using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace LCU
{
	public class ApplicationProfileManager
	{
		#region Fields
		protected IDictionary<string, ApplicationProfile> appProfiles;

        protected IConfiguration config;

        protected readonly string defaultApplicationProfileId;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public ApplicationProfileManager(IConfiguration config)
		{
            appProfiles = new Dictionary<string, ApplicationProfile>();

            this.config = config;

            defaultApplicationProfileId = "DefaultApplicationProfile";

            addDefaultApplicationProfile();
		}
		#endregion

		#region API Methods

		public virtual ApplicationProfile LoadApplicationProfile(string clientId)
		{
            ApplicationProfile appProfile = null;

            if (appProfiles.ContainsKey(clientId))
                appProfile = appProfiles[clientId];
            else
                appProfile = appProfiles[defaultApplicationProfileId];

			return appProfile;
		}

        public virtual void SaveApplicationProfile(string clientId, ApplicationProfile appProfile)
        {
            appProfiles[clientId] = appProfile;
        }
        #endregion

        #region Helpers
        protected virtual void addDefaultApplicationProfile()
        {
            appProfiles[defaultApplicationProfileId] = new ApplicationProfile()
            {
                DatabaseClientPoolSize = config["LCU-DATABASE-CLIENT-POOL-SIZE"].As<int>(4),
                DatabaseClientMaxPoolConnections = config["LCU-DATABASE-CLIENT-MAX-POOL-CONNS"].As<int>(32),
                DatabaseClientTTLMinutes = config["LCU-DATABASE-CLIENT-TTL"].As<int>(60)
            });
        }
		#endregion
	}
}
