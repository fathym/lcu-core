using Fathym;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LCU.Identity
{
	public interface IUserStore
	{
        Task<LCUUser> FindByUsername(string username);

        Task<Status> ValidateCredentials(string username, string password);

		Task<LCUUser> FindByExternalProvider(string provider, string providerUserId);

		Task<LCUUser> AutoProvisionUser(string provider, string providerUserId, List<Claim> list);
	}
}
