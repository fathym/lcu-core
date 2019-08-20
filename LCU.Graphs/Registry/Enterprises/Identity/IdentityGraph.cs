using Fathym;
using Fathym.Business.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	public class IdentityGraph : LCUGraph, IIdentityGraph
	{
		#region Properties

		#endregion

		#region Constructors
		public IdentityGraph(LCUGraphConfig config)
			: base(config)
		{ }

		public object JwtClaimTypes { get; private set; }
		#endregion

		#region API Methods
		public virtual async Task<Status> Exists(string email, string entApiKey = null)
		{
			return await withG(async (client, g) =>
			{
				var status = Status.Initialized;

				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.Has("Registry", registry)
					.Has("Email", email)
					.Out(EntGraphConstants.CarriesEdgeName)
					.HasLabel(EntGraphConstants.PassportVertexName)
					.Has("IsActive", true);

				if (!entApiKey.IsNullOrEmpty())
					existingQuery = existingQuery.Has("EnterpriseAPIKey", entApiKey);

				var accResults = await Submit<dynamic>(existingQuery);

				var accResult = accResults.FirstOrDefault();

				if (accResult != null)
					status = Status.Success;
				else
					status = Status.NotLocated;

				return status;
			});
		}

		public virtual async Task<Account> Get(string email)
		{
			return await withG(async (client, g) =>
			{
				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Registry", registry)
					.Has("Email", email);

				var accResults = await Submit<Account>(existingQuery);

				var accResult = accResults.FirstOrDefault();

				return accResult;
			});
		}

		public virtual async Task<IEnumerable<Claim>> GetClaims(string userId)
		{
			return new Claim[]
			{
				new Claim("user_id", userId ?? "")
			};
			//new Claim(JwtClaimTypes.Name, (!string.IsNullOrEmpty(user.Firstname) && !string.IsNullOrEmpty(user.Lastname)) ? (user.Firstname + " " + user.Lastname) : ""),
			//new Claim(JwtClaimTypes.GivenName, user.Firstname  ?? ""),
			//new Claim(JwtClaimTypes.FamilyName, user.Lastname  ?? ""),
			//new Claim(JwtClaimTypes.Email, user.Email  ?? ""),
			//new Claim("some_claim_you_want_to_see", user.Some_Data_From_User ?? ""),

			//roles
			//new Claim(JwtClaimTypes.Role, user.Role)
		}

		public virtual async Task<AccessCard> GetAccessCard(string entApiKey, string username, string accessConfigType)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccessCardVertexName)
					.Has("Registry", $"{entApiKey}|{username}")
					.Has("EnterpriseAPIKey", entApiKey)
					.Has("AccessConfigurationType", accessConfigType);

				var acResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				acResult.Metadata["ExcludeAccessRightIDs"] = acResult.Metadata["ExcludeAccessRightIDs"].ToString().FromJSON<JToken>();

				acResult.Metadata["IncludeAccessRightIDs"] = acResult.Metadata["IncludeAccessRightIDs"].ToString().FromJSON<JToken>();

				var accessCard = acResult.JSONConvert<AccessCard>();

				return accessCard;
			});
		}

		public virtual async Task<RelyingParty> GetRelyingParty(string entApiKey)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.RelyingPartyVertexName)
					.Has("Registry", entApiKey)
					.Has("EnterpriseAPIKey", entApiKey);

				var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

				rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

				rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

				var relyingParty = rpResult.JSONConvert<RelyingParty>();

				return relyingParty;
			});
		}

		public virtual async Task<List<AccessCard>> ListAccessCards(string entApiKey, string username)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccessCardVertexName)
					.Has("Registry", $"{entApiKey}|{username}")
					.Has("EnterpriseAPIKey", entApiKey);

				var acResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var accessCards = acResults.Select(acResult =>
				{
					acResult.Metadata["ExcludeAccessRightIDs"] = acResult.Metadata["ExcludeAccessRightIDs"].ToString().FromJSON<JToken>();

					acResult.Metadata["IncludeAccessRightIDs"] = acResult.Metadata["IncludeAccessRightIDs"].ToString().FromJSON<JToken>();

					return acResult.JSONConvert<AccessCard>();
				}).ToList();

				return accessCards;
			});
		}

		public virtual async Task<Status> Register(string entApiKey, string email, string password)
		{
			return await withG(async (client, g) =>
			{
				var status = Status.Initialized;

				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Registry", registry)
					.Has("Email", email);

				var existingResults = await Submit<Account>(existingQuery);

				var existingAccResult = existingResults.FirstOrDefault();

				if (existingAccResult == null)
				{
					var query = g.AddV(EntGraphConstants.AccountVertexName)
						.Property("Email", email)
						.Property("Registry", registry);

					var accResults = await Submit<Account>(query);

					existingAccResult = accResults.FirstOrDefault();
				}

				var existingPassportQuery = g.V(existingAccResult.ID)
						.Out(EntGraphConstants.CarriesEdgeName)
						.HasLabel(EntGraphConstants.PassportVertexName)
						.Has("EnterpriseAPIKey", entApiKey)
						.Has("Registry", $"{entApiKey}|{registry}");

				var existingPassportResults = await Submit<Passport>(existingPassportQuery);

				var existingPassportResult = existingPassportResults.FirstOrDefault();

				if (existingPassportResult == null)
				{
					var passportQuery = g.AddV(EntGraphConstants.PassportVertexName)
					.Property("PasswordHash", password.ToMD5Hash())
					.Property("EnterpriseAPIKey", entApiKey)
					.Property("IsActive", true)
					.Property("Registry", $"{entApiKey}|{registry}");

					var passportResults = await Submit<Passport>(passportQuery);

					existingPassportResult = passportResults.FirstOrDefault();

					var edgeQueries = new[] {
						g.V(existingAccResult.ID).AddE(EntGraphConstants.CarriesEdgeName).To(g.V(existingPassportResult.ID)),
					};

					foreach (var edgeQuery in edgeQueries)
					{
						await Submit(edgeQuery);
					}

					status = Status.Success;
				}
				else
					return Status.Conflict.Clone("Passport already exists.");

				if (!status)
					return Status.GeneralError.Clone("There was an issue registering the current account.");

				return status;
			});
		}

		public virtual async Task<string> RetrieveThirdPartyAccessToken(string entApiKey, string email, string key)
		{
			return await withG(async (client, g) =>
			{
				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Registry", registry)
					.Has("Email", email)
					.Out(EntGraphConstants.CarriesEdgeName)
					.HasLabel(EntGraphConstants.PassportVertexName)
					.Has("EnterpriseAPIKey", entApiKey)
					.Has("Registry", $"{entApiKey}|{registry}")
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
					.Has("Registry", email)
					.Has("Key", key);

				var tptResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var tptResult = tptResults.FirstOrDefault();

				return tptResult?.Metadata["Token"].ToString();
			});
		}

		public virtual async Task<Status> SetThirdPartyAccessToken(string entApiKey, string email, string key, string token)
		{
			return await withG(async (client, g) =>
			{
				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Registry", registry)
					.Has("Email", email)
					.Out(EntGraphConstants.CarriesEdgeName)
					.HasLabel(EntGraphConstants.PassportVertexName)
					.Has("EnterpriseAPIKey", entApiKey)
					.Has("Registry", $"{entApiKey}|{registry}")
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
					.Has("Registry", email)
					.Has("Key", key);

				var tptResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var tptResult = tptResults.FirstOrDefault();

				var setQuery = tptResult != null ? existingQuery :
					g.AddV(EntGraphConstants.ThirdPartyTokenVertexName)
						.Property("Registry", email)
						.Property("Key", key);

				setQuery = setQuery.Property("Token", token);

				tptResults = await Submit<BusinessModel<Guid>>(setQuery);

				tptResult = tptResults.FirstOrDefault();

				var passQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Registry", registry)
					.Has("Email", email)
					.Out(EntGraphConstants.CarriesEdgeName)
					.HasLabel(EntGraphConstants.PassportVertexName)
					.Has("EnterpriseAPIKey", entApiKey)
					.Has("Registry", $"{entApiKey}|{registry}");

				var passResults = await Submit<Passport>(passQuery);

				var passResult = passResults.FirstOrDefault();

				var edgeResults = await Submit<BusinessModel<Guid>>(g.V(passResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(tptResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
							g.V(passResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(tptResult.ID)),
						};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return Status.Success;
			});
		}

		public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entApiKey, string username)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccessCardVertexName)
					.Has("Registry", $"{entApiKey}|{username}")
					.Has("EnterpriseAPIKey", entApiKey)
					.Has("AccessConfigurationType", accessCard.AccessConfigurationType);

				var acResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var setQuery = acResult != null ? existingQuery :
					g.AddV(EntGraphConstants.AccessCardVertexName)
						.Property("Registry", $"{entApiKey}|{username}")
						.Property("EnterpriseAPIKey", entApiKey)
						.Property("AccessConfigurationType", accessCard.AccessConfigurationType);

				setQuery = setQuery
					.Property("ExcludeAccessRightIDs", accessCard.ExcludeAccessRights.ToJSON())
					.Property("IncludeAccessRightIDs", accessCard.IncludeAccessRights.ToJSON())
					.Property("ProviderID", accessCard.ProviderID);

				acResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

				if (acResult != null)
				{
					var rpQuery = g.V()
						.HasLabel(EntGraphConstants.RelyingPartyVertexName)
						.Has("Registry", entApiKey)
						.Has("EnterpriseAPIKey", entApiKey);

					var rpResult = await SubmitFirst<BusinessModel<Guid>>(rpQuery);

					if (rpResult == null)
					{
						var registry = username.Split('@')[1];

						var accQuery = g.V()
							.HasLabel(EntGraphConstants.AccountVertexName)
							.Has("Registry", registry)
							.Has("Email", username);

						var accResult = await SubmitFirst<Account>(accQuery);

						var edgeQueries = new[] {
							g.V(rpResult.ID).AddE(EntGraphConstants.ProvidesEdgeName).To(g.V(acResult.ID)),
							g.V(accResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(acResult.ID))
						};

						foreach (var edgeQuery in edgeQueries)
							await Submit(edgeQuery);
					}

					acResult.Metadata["ExcludeAccessRightIDs"] = acResult.Metadata["ExcludeAccessRightIDs"].ToString().FromJSON<JToken>();

					acResult.Metadata["IncludeAccessRightIDs"] = acResult.Metadata["IncludeAccessRightIDs"].ToString().FromJSON<JToken>();

					accessCard = acResult.JSONConvert<AccessCard>();
				}

				return accessCard;
			});
		}

		public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entApiKey)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.RelyingPartyVertexName)
					.Has("Registry", entApiKey)
					.Has("EnterpriseAPIKey", entApiKey);

				var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var setQuery = rpResult != null ? existingQuery :
					g.AddV(EntGraphConstants.RelyingPartyVertexName)
						.Property("Registry", entApiKey)
						.Property("EnterpriseAPIKey", entApiKey);

				setQuery = setQuery
					.Property("AccessConfigurations", relyingParty.AccessConfigurations.ToJSON())
					.Property("AccessRights", relyingParty.AccessRights.ToJSON())
					.Property("DefaultAccessConfigurationType", relyingParty.DefaultAccessConfigurationType)
					.Property("Providers", relyingParty.Providers.ToJSON());

				rpResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

				if (rpResult != null)
				{
					var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
						.Has("Registry", entApiKey)
						.Has("PrimaryAPIKey", entApiKey);

					var entResult = await SubmitFirst<BusinessModel<Guid>>(entQuery);

					if (entResult == null)
					{
						var edgeQueries = new[] {
							g.V(entResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(rpResult.ID))
						};

						foreach (var edgeQuery in edgeQueries)
							await Submit(edgeQuery);
					}

					rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

					rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

					rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

					relyingParty = rpResult.JSONConvert<RelyingParty>();
				}

				return relyingParty;
			});
		}

		public virtual async Task<Status> Validate(string entApiKey, string email, string password)
		{
			return await withG(async (client, g) =>
			{
				var status = Status.Initialized;

				var registry = email.Split('@')[1];

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.AccountVertexName)
					.Has("Email", email)
					.Has("Registry", registry)
					.Out(EntGraphConstants.CarriesEdgeName)
					.HasLabel(EntGraphConstants.PassportVertexName)
					.Has("IsActive", true)
					.Has("PasswordHash", password.ToMD5Hash())
					.Has("Registry", $"{entApiKey}|{registry}");

				var accResults = await Submit<Passport>(existingQuery);

				var accResult = accResults.FirstOrDefault();

				if (accResult != null)
					status = Status.Success;
				else
					status = Status.Unauthorized;

				return status;
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
