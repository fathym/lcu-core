﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace LCU
{
	public class ApplicationProfileManager
	{
		#region Fields
		protected readonly IDictionary<string, ApplicationProfile> appProfiles;

        protected static string defaultApplicationProfileId;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public ApplicationProfileManager()
		{
            appProfiles = new Dictionary<string, ApplicationProfile>();

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

        public virtual void SaveApplicationProfile(string appId, ApplicationProfile appProfile)
        {
            appProfiles[appId] = appProfile;
        }
        #endregion

        #region Helpers
        protected virtual void addDefaultApplicationProfile()
        {
            appProfiles.Add(defaultApplicationProfileId, new ApplicationProfile()
            {
                DatabaseClientPoolSize = 4,
                DatabaseClientMaxPoolConnections = 32,
                DatabaseClientTTLMinutes = 60
            });
        }
		#endregion
	}
}
