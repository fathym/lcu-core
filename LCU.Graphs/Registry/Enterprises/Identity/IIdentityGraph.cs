using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Fathym;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	public interface IIdentityGraph
	{
		Task<Status> Exists(string email, string entLookup = null);

		Task<Account> Get(string email);

		Task<IEnumerable<Claim>> GetClaims(string userId);

		Task<Status> Register(string entLookup, string email, string password);

		Task<string> RetrieveThirdPartyAccessToken(string entLookup, string email, string key);

		Task<Status> SetThirdPartyAccessToken(string entLookup, string email, string key, string token, string encrypt);

		Task<Status> Validate(string entLookup, string email, string password);
	}
}