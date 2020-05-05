using Fathym;
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
