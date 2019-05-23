using Fathym;
using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Identity;
using LCU.Presentation.Enterprises;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity.Core
{
    public class LCUUserStore : IUserStore
    {
        #region Fields
        protected readonly IHttpContextAccessor httpContext;

        protected readonly IdentityGraph identity;
        #endregion

        #region Constructors
        public LCUUserStore(IdentityGraph identity, IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;

            this.identity = identity;
        }
		#endregion

		#region API Methods
		public virtual async Task<LCUUser> FindByUsername(string username)
        {
            var graph = await identity.Get(username);

            return mapGraphToUser(graph);
        }

        public virtual async Task<Status> ValidateCredentials(string username, string password)
        {
            var entCtx = httpContext.HttpContext.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

            return await identity.Validate(entCtx.PrimaryAPIKey, username, password);
		}

		public virtual async Task<LCUUser> AutoProvisionUser(string provider, string providerUserId, List<Claim> list)
		{
			throw new NotImplementedException();
		}

		public virtual async Task<LCUUser> FindByExternalProvider(string provider, string providerUserId)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Helpers
		protected virtual LCUUser mapGraphToUser(Account graph)
        {
            var user = new LCUUser()
            {
                Username = graph.Email,
                SubjectId = graph.Email.ToMD5Hash(),
                Claims = new[]
                    {
                        new Claim("user_id", graph.Email)
                    }
            };

            return user;
        }
        #endregion
    }
}
