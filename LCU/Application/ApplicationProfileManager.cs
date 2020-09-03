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

        protected readonly int databaseClientMaxPoolConnections;

        protected readonly int databaseClientPoolSize;

        protected readonly int databaseClientTTLMinutes;

        protected readonly string defaultApplicationProfileId;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public ApplicationProfileManager(IConfiguration config)
            : this(config["LCU-DATABASE-CLIENT-MAX-POOL-CONNS"].As<int>(32), config["LCU-DATABASE-CLIENT-POOL-SIZE"].As<int>(4),
                config["LCU-DATABASE-CLIENT-TTL"].As<int>(60))
		{
            
		}

        public ApplicationProfileManager(int DatabaseClientMaxPoolConnections, int DatabaseClientPoolSize, int DatabaseClientTTLMinutes)
        {
            this.databaseClientMaxPoolConnections = DatabaseClientMaxPoolConnections;

            this.databaseClientPoolSize = DatabaseClientPoolSize;

            this.databaseClientTTLMinutes = DatabaseClientTTLMinutes;

            appProfiles = new Dictionary<string, ApplicationProfile>();

            defaultApplicationProfileId = "DefaultApplicationProfile";

            addDefaultApplicationProfile();
        }
		#endregion

		#region API Methods

		public virtual ApplicationProfile LoadApplicationProfile(string clientId)
		{
            ApplicationProfile appProfile = null;

            lock (appProfiles)
            {
                if (appProfiles.ContainsKey(clientId))
                    appProfile = appProfiles[clientId];
                else
                    appProfile = appProfiles[defaultApplicationProfileId];
            }

			return appProfile;
		}

        public virtual void SaveApplicationProfile(string clientId, ApplicationProfile appProfile)
        {
            lock (appProfiles)
            {
                appProfiles[clientId] = appProfile;
            }

        }
        #endregion

        #region Helpers
        protected virtual void addDefaultApplicationProfile()
        {
            appProfiles[defaultApplicationProfileId] = new ApplicationProfile()
            {
                DatabaseClientPoolSize = databaseClientPoolSize,
                DatabaseClientMaxPoolConnections = databaseClientMaxPoolConnections,
                DatabaseClientTTLMinutes = databaseClientTTLMinutes
            };
        }
		#endregion
	}
}
