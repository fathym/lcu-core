using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Fathym;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	public interface IIdentityGraph
	{
		Task<Status> Exists(string email, string entApiKey = null);

		Task<Account> Get(string email);

		Task<IEnumerable<Claim>> GetClaims(string userId);

		Task<Status> Register(string entApiKey, string email, string password);

		Task<string> RetrieveThirdPartyAccessToken(string entApiKey, string email, string key);

		Task<Status> SetThirdPartyAccessToken(string entApiKey, string email, string key, string token, string encrypt);

		Task<Status> Validate(string entApiKey, string email, string password);
	}
}