//using Fathym;
//using Fathym.Business.Models;
//using Gremlin.Net.Structure;
//using Gremlin.Net.Process.Traversal;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using LCU.Security;
//using Microsoft.Extensions.Configuration;

//namespace LCU.Graphs.Registry.Enterprises.Identity
//{
//	public class IdentityGraph : LCUGraph
//	{
//		#region Properties
//		#endregion

//		#region Constructors
//		public IdentityGraph(GremlinClientPoolManager clientPool)
//			: base(clientPool)
//		{ }

//		public object JwtClaimTypes { get; private set; }
//		#endregion

//		#region API Methods
//		public virtual async Task<Status> DeleteAccessCard(string entLookup, string username, string accessConfigType)
//		{
//			return await withG(async (client, g) =>
//			{
//				var dropQuery = g.V()
//					.HasLabel(EntGraphConstants.AccessCardVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Has("AccessConfigurationType", accessConfigType)
//					.Drop();

//				await Submit(dropQuery);

//				return Status.Success;
//			}, entLookup);
//		}

//		public virtual async Task<Status> DeleteRelyingParty(string entLookup)
//		{
//			return await withG(async (client, g) =>
//			{
//				var dropQuery = g.V()
//					.HasLabel(EntGraphConstants.RelyingPartyVertexName)
//					.Has(EntGraphConstants.RegistryName, entLookup)
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Drop();

//				await Submit(dropQuery);

//				return Status.Success;
//			}, entLookup);
//		}

//		public virtual async Task<Status> Exists(string email, string entLookup = null)
//		{
//			return await withG(async (client, g) =>
//			{
//				var status = Status.Initialized;

//				var registry = email.Split('@')[1];

//				var existingQuery = g.V()
//					.Has(EntGraphConstants.RegistryName, registry)
//					.Has("Email", email)
//					.Out(EntGraphConstants.CarriesEdgeName)
//					.HasLabel(EntGraphConstants.PassportVertexName)
//					.Has("IsActive", true);

//				if (!entLookup.IsNullOrEmpty())
//					existingQuery = existingQuery.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var accResult = await SubmitFirst<dynamic>(existingQuery);

//				if (accResult != null)
//					status = Status.Success;
//				else
//					status = Status.NotLocated;

//				return status;
//			}, entLookup);
//		}

//		public virtual async Task<Account> Get(string email)
//		{
//			return await withG(async (client, g) =>
//			{
//				var registry = email.Split('@')[1];

//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccountVertexName)
//					.Has(EntGraphConstants.RegistryName, registry)
//					.Has("Email", email);

//				var accResult = await SubmitFirst<Account>(existingQuery);

//				return accResult;
//			});
//		}

//		public virtual async Task<Passport> GetPassport(string email, string entApiKey = null)
//		{
//			return await withG(async (client, g) =>
//			{
//				var registry = email.Split('@')[1];

//				var existingQuery = g.V()
//					.Has(EntGraphConstants.RegistryName, registry)
//					.Has("Email", email)
//					.Out(EntGraphConstants.CarriesEdgeName)
//					.HasLabel(EntGraphConstants.PassportVertexName)
//					.Has("IsActive", true);

//				if (!entApiKey.IsNullOrEmpty())
//					existingQuery = existingQuery.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

//				var passportResult = await SubmitFirst<Passport>(existingQuery);

//				return passportResult;
//			}, entApiKey);
//		}

//		public virtual async Task<IEnumerable<Claim>> GetClaims(string userId)
//		{
//			return new Claim[]
//			{
//				new Claim("user_id", userId ?? "")
//			};
//			//new Claim(JwtClaimTypes.Name, (!string.IsNullOrEmpty(user.Firstname) && !string.IsNullOrEmpty(user.Lastname)) ? (user.Firstname + " " + user.Lastname) : ""),
//			//new Claim(JwtClaimTypes.GivenName, user.Firstname  ?? ""),
//			//new Claim(JwtClaimTypes.FamilyName, user.Lastname  ?? ""),
//			//new Claim(JwtClaimTypes.Email, user.Email  ?? ""),
//			//new Claim("some_claim_you_want_to_see", user.Some_Data_From_User ?? ""),

//			//roles
//			//new Claim(JwtClaimTypes.Role, user.Role)
//		}

//		public virtual async Task<AccessCard> GetAccessCard(string entLookup, string username, string accessConfigType)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccessCardVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Has("AccessConfigurationType", accessConfigType);

//				var acResult = await SubmitFirst<AccessCard>(existingQuery);

//				return acResult;
//			}, entLookup);
//		}

//		public virtual async Task<RelyingParty> GetRelyingParty(string entLookup)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.RelyingPartyVertexName)
//					.Has(EntGraphConstants.RegistryName, entLookup)
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

//				rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

//				rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

//				rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

//				var relyingParty = rpResult.JSONConvert<RelyingParty>();

//				return relyingParty;
//			}, entLookup);
//		}

//		public virtual async Task<LicenseAccessToken> GetLicenseAccessToken(string entLookup, string username, string lookup)
//		{
//			return await withG(async (client, g) =>
//			{

//				// Check for existing token
//				var existingQuery = g.V()
//				 .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//				 .Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//				 .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//				 .Has("Lookup", lookup);

//				var tokResult = await SubmitFirst<LicenseAccessToken>(existingQuery);

//				return tokResult;

//			}, entLookup);
//		}

//		public virtual async Task<List<string>> ListAccountsByOrg(string entLookup)
//		{
//			return await withG(async (client, g) =>
//			{

//				var query = g.V().HasLabel(EntGraphConstants.PassportVertexName)
//								.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//								.InE(EntGraphConstants.CarriesEdgeName)
//								.OutV()
//								.HasLabel(EntGraphConstants.AccountVertexName);

//				var results = await Submit<Account>(query);

//				var members = new List<string>();

//				foreach (var result in results)
//					members.Add(result.Email);

//				return members;
//			});
//		}

//		public virtual async Task<List<AccessCard>> ListAccessCards(string entLookup, string username)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccessCardVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var acResults = await Submit<AccessCard>(existingQuery);

//				return acResults?.ToList();
//			}, entLookup);
//		}

//		public virtual async Task<List<string>> ListAdmins(string entLookup)
//		{
//			return await ListMembersWithAccessConfigType(entLookup, EntGraphConstants.AccessConfigurationRoleAdmin);
//		}

//		public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokens(string entLookup)
//		{
//			return await withG(async (client, g) =>
//			{
//				var tokenQuery = g.V()
//				 .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//				 .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var tokResult = await Submit<LicenseAccessToken>(tokenQuery);

//				var tokResultList = tokResult?.ToList<LicenseAccessToken>();

//				return tokResultList;
//			}, entLookup);
//		}

//		public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokensByUser(string entLookup, string username)
//		{
//			return await withG(async (client, g) =>
//			{

//				// Check for existing token
//				var existingQuery = g.V()
//				 .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//				 .Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//				 .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var tokResult = await Submit<LicenseAccessToken>(existingQuery);

//				return tokResult.ToList<LicenseAccessToken>();

//			}, entLookup);
//		}

//		public virtual async Task<List<string>> ListMembersWithAccessConfigType(string entLookup, string accessConfigType)
//		{
//			return await withG(async (client, g) =>
//			{
//				var members = new List<string>();

//				var query = g.V().HasLabel(EntGraphConstants.AccessCardVertexName)
//								.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//								.Has(EntGraphConstants.AccessConfigurationTypeName, accessConfigType);

//		var results = await Submit<AccessCard>(query);

//		foreach (var result in results)
//			if (result.Registry?.Split('|').Count() > 1)
//				members.Add(result.Registry.Split('|')[1]);

//		return members;
//	});
//}

//public virtual async Task<Status> Register(string entApiKey, string email, string password, string providerId)
//{
//	return await withG(async (client, g) =>
//	{
//		var status = Status.Initialized;

//		var registry = email.Split('@')[1];

//		var existingQuery = g.V()
//			.HasLabel(EntGraphConstants.AccountVertexName)
//			.Has(EntGraphConstants.RegistryName, registry)
//			.Has("Email", email);

//				var existingAccResult = await SubmitFirst<Account>(existingQuery);

//				if (existingAccResult == null)
//				{
//					var query = g.AddV(EntGraphConstants.AccountVertexName)
//						.Property("Email", email)
//						.Property(EntGraphConstants.RegistryName, registry);

//					existingAccResult = await SubmitFirst<Account>(query);
//				}

//				var existingPassportQuery = g.V(existingAccResult.ID)
//						.Out(EntGraphConstants.CarriesEdgeName)
//						.HasLabel(EntGraphConstants.PassportVertexName)
//						.Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}")
//						.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var existingPassportResult = await SubmitFirst<Passport>(existingPassportQuery);

//				if (existingPassportResult == null)
//				{
//					var passportQuery = g.AddV(EntGraphConstants.PassportVertexName)
//					.Property(EntGraphConstants.RegistryName, $"{entLookup}|{registry}")
//					.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Property("PasswordHash", password.ToMD5Hash())
//					.Property("ProviderID", providerId)
//					.Property("IsActive", true);

//					existingPassportResult = await SubmitFirst<Passport>(passportQuery);

//					await ensureEdgeRelationships(g, existingAccResult.ID, existingPassportResult.ID,
//						edgeToCheckBuy: EntGraphConstants.CarriesEdgeName, edgesToCreate: new List<string>()
//						{
//							EntGraphConstants.CarriesEdgeName
//						});

//					status = Status.Success;
//				}
//				else
//				{
//					var updatePassportQuery = g.V(existingPassportResult.ID)
//					.Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}")
//					.Property("PasswordHash", password.ToMD5Hash())
//					.Property("ProviderID", providerId);

//					existingPassportResult = await SubmitFirst<Passport>(updatePassportQuery);

//					status = Status.Success;
//				}

//				if (!status)
//					return Status.GeneralError.Clone("There was an issue registering the current account.");

//				return status;
//			}, entLookup);
//		}

//		public virtual async Task<Status> RemoveLicenseAccessToken(string entLookup, string username, string lookup)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Has("Lookup", lookup)
//					.Drop();

//				await Submit(existingQuery);

//				return Status.Success;

//			}, entLookup);
//		}

//		public virtual async Task<string> RetrieveThirdPartyAccessToken(string entLookup, string email, string key, string tokenEncodingKey)
//		{
//			return await withG(async (client, g) =>
//			{
//				bool isEncrypted = false;

//				string tokenResult = String.Empty;

//				var registry = email.Split('@')[1];

//				if (!entLookup.IsNullOrEmpty())
//				{
//					var existingEntQuery = g.V()
//						.HasLabel(EntGraphConstants.AccountVertexName)
//						.Has(EntGraphConstants.RegistryName, registry)
//						.Has("Email", email)
//						.Out(EntGraphConstants.CarriesEdgeName)
//						.HasLabel(EntGraphConstants.PassportVertexName)
//						.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//						.Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}")
//						.Out(EntGraphConstants.OwnsEdgeName)
//						.HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
//						.Has(EntGraphConstants.RegistryName, email)
//						.Has("Key", key);

//					var entTptResult = await SubmitFirst<BusinessModel<Guid>>(existingEntQuery);

//					//return entTptResult?.Metadata["Token"].ToString();

//					if (entTptResult != null && entTptResult.Metadata.ContainsKey("Encrypt"))
//						isEncrypted = entTptResult.Metadata["Encrypt"].ToObject<bool>();

//					tokenResult = isEncrypted ?
//						Encryption.Decrypt(entTptResult?.Metadata["Token"].ToString(), tokenEncodingKey) :
//						entTptResult?.Metadata["Token"].ToString();

//				}
//				else
//				{
//					var existingAccQuery = g.V()
//						.HasLabel(EntGraphConstants.AccountVertexName)
//						.Has(EntGraphConstants.RegistryName, registry)
//						.Has("Email", email)
//						.Out(EntGraphConstants.OwnsEdgeName)
//						.HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
//						.Has(EntGraphConstants.RegistryName, email)
//						.Has("Key", key);

//					var accTptResult = await SubmitFirst<BusinessModel<Guid>>(existingAccQuery);

//					if (accTptResult != null && accTptResult.Metadata.ContainsKey("Encrypt"))
//						isEncrypted = accTptResult.Metadata["Encrypt"].ToObject<bool>();

//					tokenResult = isEncrypted ?
//						Encryption.Decrypt(accTptResult?.Metadata["Token"].ToString(), tokenEncodingKey) :
//						accTptResult?.Metadata["Token"].ToString();

//				}

//				return tokenResult;
//			}, entLookup);
//		}

//		public virtual async Task<Status> SetThirdPartyAccessToken(string entLookup, string email, string key, string token, bool encrypt = false)
//		{
//			return await withG(async (client, g) =>
//			{
//				var registry = email.Split('@')[1];

//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccountVertexName)
//					.Has(EntGraphConstants.RegistryName, registry)
//					.Has("Email", email);

//				if (!entLookup.IsNullOrEmpty())
//				{
//					existingQuery = existingQuery
//					.Out(EntGraphConstants.CarriesEdgeName)
//					.HasLabel(EntGraphConstants.PassportVertexName)
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}");
//				}

//				existingQuery = existingQuery
//					.Out(EntGraphConstants.OwnsEdgeName)
//					.HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
//					.Has(EntGraphConstants.RegistryName, email)
//					.Has("Key", key);

//				var tptResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

//				var setQuery = tptResult != null ? existingQuery :
//					g.AddV(EntGraphConstants.ThirdPartyTokenVertexName)
//						.Property(EntGraphConstants.RegistryName, email)
//						.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//						.Property("Key", key);

//				setQuery = setQuery.Property("Token", token)
//					.Property("Encrypt", encrypt);

//				tptResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

//				if (!entLookup.IsNullOrEmpty())
//				{
//					var passQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
//						.Has(EntGraphConstants.RegistryName, registry)
//						.Has("Email", email)
//						.Out(EntGraphConstants.CarriesEdgeName)
//						.HasLabel(EntGraphConstants.PassportVertexName)
//						.Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}")
//						.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//					var passResult = await SubmitFirst<Passport>(passQuery);

//					await ensureEdgeRelationships(g, passResult.ID, tptResult.ID,
//						edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
//						{
//								EntGraphConstants.OwnsEdgeName
//						});
//				}
//				else
//				{
//					var accQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
//						.Has(EntGraphConstants.RegistryName, registry)
//						.Has("Email", email);

//					var accResult = await SubmitFirst<Passport>(accQuery);

//					await ensureEdgeRelationships(g, accResult.ID, tptResult.ID,
//						edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
//						{
//								EntGraphConstants.OwnsEdgeName
//						});
//				}

//				return Status.Success;
//			}, entLookup);
//		}

//		public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entLookup, string username)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccessCardVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//					.Has("AccessConfigurationType", accessCard.AccessConfigurationType);

//				var acResult = await SubmitFirst<AccessCard>(existingQuery);

//				var setQuery = acResult != null ? existingQuery :
//					g.AddV(EntGraphConstants.AccessCardVertexName)
//						.Property(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
//						.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
//						.Property("AccessConfigurationType", accessCard.AccessConfigurationType);

//				setQuery = setQuery
//					.Property("ProviderID", accessCard.ProviderID);

//				accessCard.ExcludeAccessRights.Each(ear =>
//				{
//					if (ear == accessCard.ExcludeAccessRights.First())
//						setQuery = setQuery.Property("ExcludeAccessRights", ear);
//					else
//						setQuery = setQuery.Property(Cardinality.List, "ExcludeAccessRights", ear);
//				});

//				accessCard.IncludeAccessRights.Each(iar =>
//				{
//					if (iar == accessCard.IncludeAccessRights.First())
//						setQuery = setQuery.Property("IncludeAccessRights", iar);
//					else
//						setQuery = setQuery.Property(Cardinality.List, "IncludeAccessRights", iar);
//				});

//				acResult = await SubmitFirst<AccessCard>(setQuery);

//				if (acResult != null)
//				{
//					var rpQuery = g.V()
//						.HasLabel(EntGraphConstants.RelyingPartyVertexName)
//						.Has(EntGraphConstants.RegistryName, entLookup)
//						.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//					var rpResult = await SubmitFirst<BusinessModel<Guid>>(rpQuery);

//					if (rpResult != null)
//					{
//						var registry = username.Split('@')[1];

//						var accQuery = g.V()
//							.HasLabel(EntGraphConstants.AccountVertexName)
//							.Has(EntGraphConstants.RegistryName, registry)
//							.Has("Email", username);

//						var accResult = await SubmitFirst<Account>(accQuery);

//						await ensureEdgeRelationships(g, rpResult.ID, acResult.ID,
//							edgeToCheckBuy: EntGraphConstants.ProvidesEdgeName, edgesToCreate: new List<string>()
//							{
//								EntGraphConstants.ProvidesEdgeName
//							});

//						await ensureEdgeRelationships(g, accResult.ID, acResult.ID,
//							edgeToCheckBuy: EntGraphConstants.ConsumesEdgeName, edgesToCreate: new List<string>()
//							{
//								EntGraphConstants.ConsumesEdgeName
//							});
//					}

//					accessCard = acResult;
//				}

//				return accessCard;
//			}, entLookup);
//		}

//		public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entLookup)
//		{
//			return await withG(async (client, g) =>
//			{
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.RelyingPartyVertexName)
//					.Has(EntGraphConstants.RegistryName, entLookup)
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

//				var setQuery = rpResult != null ? existingQuery :
//					g.AddV(EntGraphConstants.RelyingPartyVertexName)
//						.Property(EntGraphConstants.RegistryName, entLookup)
//						.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

//				setQuery = setQuery
//					.Property("AccessConfigurations", relyingParty.AccessConfigurations.ToJSON())
//					.Property("AccessRights", relyingParty.AccessRights.ToJSON())
//					.Property("DefaultAccessConfigurationType", relyingParty.DefaultAccessConfigurationType)
//					.Property("Providers", relyingParty.Providers.ToJSON());

//				rpResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

//				if (rpResult != null)
//				{
//					var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
//						.Has(EntGraphConstants.RegistryName, entLookup)
//						.Has("PrimaryAPIKey", entLookup);

//					var entResult = await SubmitFirst<BusinessModel<Guid>>(entQuery);

//					await ensureEdgeRelationships(g, entResult.ID, rpResult.ID,
//						edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
//						{
//							EntGraphConstants.OwnsEdgeName
//						});

//					rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

//					rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

//					rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

//					relyingParty = rpResult.JSONConvert<RelyingParty>();
//				}

//				return relyingParty;
//			}, entLookup);
//		}

//		public virtual async Task<LicenseAccessToken> SetLicenseAccessToken(LicenseAccessToken token)
//		{
//			return await withG(async (client, g) =>
//			{
//				// Check for existing token
//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//					.Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
//					.Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
//					.Has("Lookup", token.Lookup);

//				var tokResult = await SubmitFirst<LicenseAccessToken>(existingQuery);

//				// If token already exists, apply updates to license properties 
//				if (tokResult != null)
//				{
//					var accDate = tokResult.AccessStartDate;
//					var expDate = tokResult.ExpirationDate;

//					if (token.IsLocked) expDate = System.DateTime.Now;

//					if (token.IsReset)
//					{
//						accDate = System.DateTime.Now;

//						if (token.TrialPeriodDays > 0)
//							expDate = DateTime.Now.AddDays(token.TrialPeriodDays);
//					}

//					var setQuery =
//						g.V().HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
//						.Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
//						.Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
//						.Property("AccessStartDate", accDate)
//						.Property("ExpirationDate", expDate)
//						.Property("Lookup", token.Lookup)
//						.Property("TrialPeriodDays", token.TrialPeriodDays)
//						.Property("Username", token.Username)
//						.AttachMetadataProperties<LicenseAccessToken>(token);

//					var updateResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

//					tokResult = updateResult.JSONConvert<LicenseAccessToken>();
//				}
//				else
//				{
//					var expDays = token.TrialPeriodDays > 0 ? token.TrialPeriodDays : 31;  //  Exp Should not be set without trial period days? 

//					// If not, create the license access token 
//					var setQuery =
//						g.AddV(EntGraphConstants.LicenseAccessTokenVertexName)
//							.Property(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
//							.Property(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
//							.Property("AccessStartDate", DateTime.Now)
//							.Property("ExpirationDate", DateTime.Now.AddDays(expDays))
//							.Property("Lookup", token.Lookup)
//							.Property("TrialPeriodDays", token.TrialPeriodDays)
//							.Property("Username", token.Username)
//							.AttachMetadataProperties<LicenseAccessToken>(token);

//					tokResult = await SubmitFirst<LicenseAccessToken>(setQuery);

//				}
//				return tokResult;
//			}, token.EnterpriseAPIKey);
//		}

//		public virtual async Task<Status> Validate(string entLookup, string email, string password)
//		{
//			return await withG(async (client, g) =>
//			{
//				var status = Status.Initialized;

//				var registry = email.Split('@')[1];

//				var existingQuery = g.V()
//					.HasLabel(EntGraphConstants.AccountVertexName)
//					.Has("Email", email)
//					.Has(EntGraphConstants.RegistryName, registry)
//					.Out(EntGraphConstants.CarriesEdgeName)
//					.HasLabel(EntGraphConstants.PassportVertexName)
//					.Has("IsActive", true)
//					.Has("PasswordHash", password.ToMD5Hash())
//					.Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}");

//				var accResult = await SubmitFirst<Passport>(existingQuery);

//				if (accResult != null)
//					status = Status.Success;
//				else
//					status = Status.Unauthorized;

//				return status;
//			}, entLookup);
//		}
//		#endregion

//		#region Helpers
//		#endregion
//	}
//}
