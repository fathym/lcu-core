using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using LCU.Graphs.Registry.Enterprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity.Core
{
	public class LCUClientStore : IClientStore
	{
		#region Fields
		
		#endregion

		#region Constructors
		public LCUClientStore()
		{
			
		}
        #endregion

        #region API Methods
        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            return new Client
            {
                ClientId = "lcu",
                AllowedGrantTypes = GrantTypes.Hybrid,
                ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },
                AllowOfflineAccess = true,
                RedirectUris = { "http://localhost:52235/signin-oidc" },
                PostLogoutRedirectUris = { "http://localhost:52235/.identity/signout-callback-oidc" },
            };
        }
        #endregion
    }
}
