using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Structure;
using Gremlin.Net.Process.Traversal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LCU.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LCU.Graphs.Registry.Enterprises.Edges;
using ExRam.Gremlinq.Core;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
    public class IdentityGraph : LCUGraph
    {
        #region Properties
        public object JwtClaimTypes { get; private set; }
        #endregion

        #region Constructors
        public IdentityGraph(LCUGraphConfig graphConfig, ILogger<IdentityGraph> logger)
            : base(graphConfig, logger)
        { }

        #endregion

        #region API Methods
        public virtual async Task<Status> DeleteAccessCard(string entLookup, string username, string accessConfigType)
        {
            var ac = await GetAccessCard(entLookup, username, accessConfigType);

            if (ac != null)
            {
                await g.V<AccessCard>(ac.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate access card");
            }
        }

        public virtual async Task<Status> DeleteRelyingParty(string entLookup)
        {
            var rp = await GetRelyingParty(entLookup);

            if (rp != null)
            {
                await g.V<RelyingParty>(rp.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate relying party");
            }
        }

        public virtual async Task<Status> DeleteLicenseAccessToken(string entLookup, string username, string lookup)
        {
            var lat = await GetLicenseAccessToken(entLookup, username, lookup);

            if (lat != null)
            {
                await g.V<LicenseAccessToken>(lat.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate license access token");
            }
        }

        public virtual async Task<Status> Exists(string email, string entLookup = null)
        {
            var registry = email.Split('@')[1];

            var existing = await GetPassport(email, entLookup);

            if (existing != null)
                return Status.Success;
            else
                return Status.NotLocated;
        }

        public virtual async Task<Account> GetAccount(string email)
        {
            var registry = email.Split('@')[1];

            return await g.V<Account>()
                  .Where(e => e.Registry == registry)
                  .Where(e => e.Email == email)
                  .FirstOrDefaultAsync();
        }

        public virtual async Task<Passport> GetPassport(string email, string entLookup = null)
        {
            var registry = email.Split('@')[1];

            var passport = await g.V<Account>()
                  .Where(e => e.Registry == registry)
                  .Where(e => e.Email == email)
                  .Out<Carries>()
                  .OfType<Passport>()
                  .Where(e => e.IsActive)
                  .FirstOrDefaultAsync();

            if (!entLookup.IsNullOrEmpty() && passport?.EnterpriseLookup != entLookup)
                passport = null;

            return passport;
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

        public virtual async Task<AccessCard> GetAccessCard(string entLookup, string username, string accessConfigType)
        {
            var registry = username.Split('@')[1];

            var accessCard = await g.V<AccessCard>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .Where(e => e.AccessConfigurationType == accessConfigType)
                  .FirstOrDefaultAsync();

            return accessCard;
        }

        public virtual async Task<RelyingParty> GetRelyingParty(string entLookup)
        {
            var rp = await g.V<RelyingParty>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == entLookup)
                  .FirstOrDefaultAsync();

            return rp;
        }

        public virtual async Task<LicenseAccessToken> GetLicenseAccessToken(string entLookup, string username, string lookup)
        {
            var lat = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .Where(e => e.Lookup == lookup)
                  .FirstOrDefaultAsync();

            return lat;
        }

        public virtual async Task<List<Account>> ListAccountsByOrg(string entLookup)
        {
            var accounts = await g.V<Passport>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .InE<Carries>()
                  .OutV<Account>()
                  .ToListAsync();

            return accounts;
        }

        public virtual async Task<List<AccessCard>> ListAccessCards(string entLookup, string username)
        {
            var accessCards = await g.V<AccessCard>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .ToListAsync();

            return accessCards;
        }

        public virtual async Task<List<string>> ListAdmins(string entLookup)
        {
            return await ListMembersWithAccessConfigType(entLookup, "LCU");
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokens(string entLookup)
        {
            var lats = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .ToListAsync();

            return lats;
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokensByUser(string entLookup, string username)
        {
            var lats = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .ToListAsync();

            return lats;
        }

        public virtual async Task<List<string>> ListMembersWithAccessConfigType(string entLookup, string accessConfigType)
        {
            var accessCards = await g.V<AccessCard>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.AccessConfigurationType == accessConfigType)
                  .ToListAsync();

            var members = new List<string>();

            foreach (var result in accessCards)
                if (result.Registry?.Split('|').Count() > 1)
                    members.Add(result.Registry.Split('|')[1]);

            return accessCards.Select(ac => ac.Registry.Split('|')[1]).Distinct().ToList();
        }

        public virtual async Task<Status> Register(string entLookup, string email, string password, string providerId)
        {
            var registry = email.Split('@')[1];

            var account = await GetAccount(email);

            if (account == null)
                account = await g.AddV(new Account()
                {
                    Email = email,
                    Registry = registry
                }).FirstOrDefaultAsync();

            var passport = await GetPassport(email, entLookup);

            if (passport == null)
            {
                passport = new Passport()
                {
                    ID = Guid.NewGuid(),
                    EnterpriseLookup = entLookup,
                    Registry = $"{entLookup}|{registry}",
                    PasswordHash = password.ToMD5Hash(),
                    ProviderID = providerId,
                    IsActive = true
                };

                passport = await g.AddV(passport).FirstOrDefaultAsync();
            }
            else
            {
                passport.PasswordHash = password.ToMD5Hash();

                passport.ProviderID = providerId;

                passport = await g.V<Passport>(passport.ID)
                    .Update(passport)
                    .FirstOrDefaultAsync();
            }

            await ensureEdgeRelationship<Carries>(account.ID, passport.ID);

            return Status.Success;
        }

        public virtual async Task<string> RetrieveThirdPartyAccessToken(string entLookup, string email, string key, string tokenEncodingKey = null)
        {
            bool isEncrypted = false;

            string tokenResult = String.Empty;

            var registry = email.Split('@')[1];

            IVertexGremlinQueryBase tpiQuery = g.V<Account>()
                .Where(e => e.Registry == registry)
                .Where(e => e.Email == email);

            if (!entLookup.IsNullOrEmpty())
                tpiQuery = tpiQuery
                    .Out<Carries>()
                    .OfType<Passport>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == $"{entLookup}|{registry}");

            var tpi = await tpiQuery
                .Out<Owns>()
                .OfType<ThirdPartyToken>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == email)
                .FirstOrDefaultAsync();

            if (tpi != null)
                isEncrypted = tpi.Encrypt;

            return isEncrypted ? Encryption.Decrypt(tpi?.Token, tokenEncodingKey) : tpi?.Token;
        }

        public virtual async Task<Status> SetThirdPartyAccessToken(string entLookup, string email, string key, string token, string tokenEncodingKey = null)
        {
            var account = await GetAccount(email);

            var registry = email.Split('@')[1];

            IVertexGremlinQueryBase tpiQuery = g.V<Account>(account.ID)
                .Where(e => e.Registry == registry)
                .Where(e => e.Email == email);

            if (!entLookup.IsNullOrEmpty())
                tpiQuery = tpiQuery
                    .Out<Carries>()
                    .OfType<Passport>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == $"{entLookup}|{registry}");

            var tpi = await tpiQuery
                .Out<Owns>()
                .OfType<ThirdPartyToken>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == email)
                .Where(e => e.Key == key)
                .FirstOrDefaultAsync();

            if (!tokenEncodingKey.IsNullOrEmpty())
                token = Encryption.Encrypt(token, tokenEncodingKey);

            if (tpi == null)
            {
                tpi = new ThirdPartyToken()
                {
                    ID = Guid.NewGuid(),
                    EnterpriseLookup = entLookup,
                    Registry = $"{entLookup}|{registry}",
                    Encrypt = !tokenEncodingKey.IsNullOrEmpty(),
                    Key = key,
                    Token = token
                };

                tpi = await g.AddV(tpi).FirstOrDefaultAsync();
            }
            else
            {
                tpi.Encrypt = !tokenEncodingKey.IsNullOrEmpty();

                tpi.Token = token;

                tpi = await g.V<ThirdPartyToken>(tpi.ID)
                    .Update(tpi)
                    .FirstOrDefaultAsync();
            }
            return await withG(async (client, g) =>
            {
                existingQuery = existingQuery
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
                    .Has(EntGraphConstants.RegistryName, email)
                    .Has("Key", key);

                var tptResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

                var setQuery = tptResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.ThirdPartyTokenVertexName)
                        .Property(EntGraphConstants.RegistryName, email)
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                        .Property("Key", key);

                setQuery = setQuery.Property("Token", token)
                    .Property("Encrypt", encrypt);

                tptResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                if (!entLookup.IsNullOrEmpty())
                {
                    var passQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
                        .Has(EntGraphConstants.RegistryName, registry)
                        .Has("Email", email)
                        .Out(EntGraphConstants.CarriesEdgeName)
                        .HasLabel(EntGraphConstants.PassportVertexName)
                        .Has(EntGraphConstants.RegistryName, $"{entLookup}|{registry}")
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

                    var passResult = await SubmitFirst<Passport>(passQuery);

                    await ensureEdgeRelationships(g, passResult.ID, tptResult.ID,
                        edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
                        {
                                EntGraphConstants.OwnsEdgeName
                        });
                }
                else
                {
                    var accQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
                        .Has(EntGraphConstants.RegistryName, registry)
                        .Has("Email", email);

                    var accResult = await SubmitFirst<Passport>(accQuery);

                    await ensureEdgeRelationships(g, accResult.ID, tptResult.ID,
                        edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
                        {
                                EntGraphConstants.OwnsEdgeName
                        });
                }

                return Status.Success;
            }, entLookup);
        }

        public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entLookup, string username)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.AccessCardVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has("AccessConfigurationType", accessCard.AccessConfigurationType);

                var acResult = await SubmitFirst<AccessCard>(existingQuery);

                var setQuery = acResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.AccessCardVertexName)
                        .Property(EntGraphConstants.RegistryName, $"{entLookup}|{username}")
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                        .Property("AccessConfigurationType", accessCard.AccessConfigurationType);

                setQuery = setQuery
                    .Property("ProviderID", accessCard.ProviderID);

                accessCard.ExcludeAccessRights.Each(ear =>
                {
                    if (ear == accessCard.ExcludeAccessRights.First())
                        setQuery = setQuery.Property("ExcludeAccessRights", ear);
                    else
                        setQuery = setQuery.Property(Cardinality.List, "ExcludeAccessRights", ear);
                });

                accessCard.IncludeAccessRights.Each(iar =>
                {
                    if (iar == accessCard.IncludeAccessRights.First())
                        setQuery = setQuery.Property("IncludeAccessRights", iar);
                    else
                        setQuery = setQuery.Property(Cardinality.List, "IncludeAccessRights", iar);
                });

                acResult = await SubmitFirst<AccessCard>(setQuery);

                if (acResult != null)
                {
                    var rpQuery = g.V()
                        .HasLabel(EntGraphConstants.RelyingPartyVertexName)
                        .Has(EntGraphConstants.RegistryName, entLookup)
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

                    var rpResult = await SubmitFirst<BusinessModel<Guid>>(rpQuery);

                    if (rpResult != null)
                    {
                        var registry = username.Split('@')[1];

                        var accQuery = g.V()
                            .HasLabel(EntGraphConstants.AccountVertexName)
                            .Has(EntGraphConstants.RegistryName, registry)
                            .Has("Email", username);

                        var accResult = await SubmitFirst<Account>(accQuery);

                        await ensureEdgeRelationships(g, rpResult.ID, acResult.ID,
                            edgeToCheckBuy: EntGraphConstants.ProvidesEdgeName, edgesToCreate: new List<string>()
                            {
                                EntGraphConstants.ProvidesEdgeName
                            });

                        await ensureEdgeRelationships(g, accResult.ID, acResult.ID,
                            edgeToCheckBuy: EntGraphConstants.ConsumesEdgeName, edgesToCreate: new List<string>()
                            {
                                EntGraphConstants.ConsumesEdgeName
                            });
                    }

                    accessCard = acResult;
                }

                return accessCard;
            }, entLookup);
        }

        public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entLookup)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.RelyingPartyVertexName)
                    .Has(EntGraphConstants.RegistryName, entLookup)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

                var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

                var setQuery = rpResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.RelyingPartyVertexName)
                        .Property(EntGraphConstants.RegistryName, entLookup)
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

                setQuery = setQuery
                    .Property("AccessConfigurations", relyingParty.AccessConfigurations.ToJSON())
                    .Property("AccessRights", relyingParty.AccessRights.ToJSON())
                    .Property("DefaultAccessConfigurationType", relyingParty.DefaultAccessConfigurationType)
                    .Property("Providers", relyingParty.Providers.ToJSON());

                rpResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                if (rpResult != null)
                {
                    var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                        .Has(EntGraphConstants.RegistryName, entLookup)
                        .Has("PrimaryAPIKey", entLookup);

                    var entResult = await SubmitFirst<BusinessModel<Guid>>(entQuery);

                    await ensureEdgeRelationships(g, entResult.ID, rpResult.ID,
                        edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
                        {
                            EntGraphConstants.OwnsEdgeName
                        });

                    rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

                    rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

                    rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

                    relyingParty = rpResult.JSONConvert<RelyingParty>();
                }

                return relyingParty;
            }, entLookup);
        }

        public virtual async Task<LicenseAccessToken> SetLicenseAccessToken(LicenseAccessToken token)
        {
            return await withG(async (client, g) =>
            {
                // Check for existing token
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
                    .Has("Lookup", token.Lookup);

                var tokResult = await SubmitFirst<LicenseAccessToken>(existingQuery);

                // If token already exists, apply updates to license properties 
                if (tokResult != null)
                {
                    var accDate = tokResult.AccessStartDate;
                    var expDate = tokResult.ExpirationDate;

                    if (token.IsLocked) expDate = System.DateTime.Now;

                    if (token.IsReset)
                    {
                        accDate = System.DateTime.Now;

                        if (token.TrialPeriodDays > 0)
                            expDate = DateTime.Now.AddDays(token.TrialPeriodDays);
                    }

                    var setQuery =
                        g.V().HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                        .Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
                        .Property("AccessStartDate", accDate)
                        .Property("ExpirationDate", expDate)
                        .Property("Lookup", token.Lookup)
                        .Property("TrialPeriodDays", token.TrialPeriodDays)
                        .Property("Username", token.Username)
                        .AttachMetadataProperties<LicenseAccessToken>(token);

                    var updateResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                    tokResult = updateResult.JSONConvert<LicenseAccessToken>();
                }
                else
                {
                    var expDays = token.TrialPeriodDays > 0 ? token.TrialPeriodDays : 31;  //  Exp Should not be set without trial period days? 

                    // If not, create the license access token 
                    var setQuery =
                        g.AddV(EntGraphConstants.LicenseAccessTokenVertexName)
                            .Property(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.Username}")
                            .Property(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
                            .Property("AccessStartDate", DateTime.Now)
                            .Property("ExpirationDate", DateTime.Now.AddDays(expDays))
                            .Property("Lookup", token.Lookup)
                            .Property("TrialPeriodDays", token.TrialPeriodDays)
                            .Property("Username", token.Username)
                            .AttachMetadataProperties<LicenseAccessToken>(token);

                    tokResult = await SubmitFirst<LicenseAccessToken>(setQuery);

                }
                return tokResult;
            }, token.EnterpriseAPIKey);
        }

        public virtual async Task<Status> Validate(string entLookup, string email, string password)
        {
            var passport = await GetPassport(email, entLookup);

            if (passport != null && passport.PasswordHash == password.ToMD5Hash())
                return Status.Success;
            else
                return Status.Unauthorized;
        }
        #endregion

        #region Helpers
        #endregion
    }
}
