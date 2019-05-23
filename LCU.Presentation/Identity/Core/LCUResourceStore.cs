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
	public class LCUResourceStore : IResourceStore
	{
		#region Fields
		
		#endregion

		#region Constructors
		public LCUResourceStore()
		{
			
		}
        #endregion

        #region API Methods
        public virtual async Task<ApiResource> FindApiResourceAsync(string name)
        {
            return new ApiResource();
        }

        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
            };
        }

        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource()
                {
                    DisplayName = "user_id",
                    Name = "user_id",
                    UserClaims = new List<string>()
                    {
                        "user_id"
                    }
                }
            };
        }

        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            return new Resources();
        }
        #endregion
    }
}
