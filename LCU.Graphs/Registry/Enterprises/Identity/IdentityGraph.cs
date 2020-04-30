﻿using Fathym;
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

namespace LCU.Graphs.Registry.Enterprises.Identity
{
    public class IdentityGraph : LCUGraph, IIdentityGraph
    {
        #region Properties
        protected readonly IConfiguration config;
        #endregion

        #region Constructors
        public IdentityGraph(IConfiguration config, GremlinClientPoolManager clientPool)
            : base(clientPool)
        {
            this.config = config;
        }

        public object JwtClaimTypes { get; private set; }
        #endregion

        #region API Methods
        public virtual async Task<Status> DeleteAccessCard(string entApiKey, string username, string accessConfigType)
        {
            return await withG(async (client, g) =>
            {
                var dropQuery = g.V()
                    .HasLabel(EntGraphConstants.AccessCardVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                    .Has("AccessConfigurationType", accessConfigType)
                    .Drop();

                await Submit(dropQuery);

                return Status.Success;
            }, entApiKey);
        }

        public virtual async Task<Status> Exists(string email, string entApiKey = null)
        {
            return await withG(async (client, g) =>
            {
                var status = Status.Initialized;

                var registry = email.Split('@')[1];

                var existingQuery = g.V()
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Email", email)
                    .Out(EntGraphConstants.CarriesEdgeName)
                    .HasLabel(EntGraphConstants.PassportVertexName)
                    .Has("IsActive", true);

                if (!entApiKey.IsNullOrEmpty())
                    existingQuery = existingQuery.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var accResult = await SubmitFirst<dynamic>(existingQuery);

                if (accResult != null)
                    status = Status.Success;
                else
                    status = Status.NotLocated;

                return status;
            }, entApiKey);
        }

        public virtual async Task<Account> Get(string email)
        {
            return await withG(async (client, g) =>
            {
                var registry = email.Split('@')[1];

                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.AccountVertexName)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Email", email);

                var accResult = await SubmitFirst<Account>(existingQuery);

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
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                    .Has("AccessConfigurationType", accessConfigType);

                var acResult = await SubmitFirst<AccessCard>(existingQuery);

                return acResult;
            }, entApiKey);
        }

        public virtual async Task<RelyingParty> GetRelyingParty(string entApiKey)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.RelyingPartyVertexName)
                    .Has(EntGraphConstants.RegistryName, entApiKey)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

                rpResult.Metadata["AccessConfigurations"] = rpResult.Metadata["AccessConfigurations"].ToString().FromJSON<JToken>();

                rpResult.Metadata["AccessRights"] = rpResult.Metadata["AccessRights"].ToString().FromJSON<JToken>();

                rpResult.Metadata["Providers"] = rpResult.Metadata["Providers"].ToString().FromJSON<JToken>();

                var relyingParty = rpResult.JSONConvert<RelyingParty>();

                return relyingParty;
            }, entApiKey);
        }

        public async Task<LicenseAccessToken> GetLicenseAccessToken(string entApiKey, string username)
        {
            return await withG(async (client, g) =>
            {

                // Check for existing token
                var existingQuery = g.V()
                 .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                 .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                 .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var tokResult = await SubmitFirst<LicenseAccessToken>(existingQuery);

                return tokResult;

            }, entApiKey);
        }

        public virtual async Task<List<string>> ListAccountsByOrg(string entApiKey)
        {
            return await withG(async (client, g) =>
            {

                var query = g.V().HasLabel(EntGraphConstants.PassportVertexName)
                                .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                                .InE(EntGraphConstants.CarriesEdgeName)
                                .OutV()
                                .HasLabel(EntGraphConstants.AccountVertexName);

                var results = await Submit<Account>(query);

                var members = new List<string>();

                foreach (var result in results)
                    members.Add(result.Email);

                return members;
            });
        }

        public virtual async Task<List<AccessCard>> ListAccessCards(string entApiKey, string username)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.AccessCardVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var acResults = await Submit<AccessCard>(existingQuery);

                return acResults?.ToList();
            }, entApiKey);
        }

        public virtual async Task<List<string>> ListAdmins(string entApiKey)
        {
            return await ListMembersWithAccessConfigType(entApiKey, EntGraphConstants.AccessConfigurationRoleAdmin);
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokens(string entApiKey)
        {
            return await withG(async (client, g) =>
            {

                var tokenQuery = g.V()
                 .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                 .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var tokResult = await Submit<LicenseAccessToken>(tokenQuery);

                var tokResultList = tokResult?.ToList<LicenseAccessToken>();

                tokResultList.ForEach(p =>
                             {
                                 p.UserName = p.Registry.Split('|')[1]?.ToString();
                             });

                return tokResultList;
            }, entApiKey);
        }

        public virtual async Task<List<string>> ListMembersWithAccessConfigType(string entApiKey, string accessConfigType)
        {
            return await withG(async (client, g) =>
            {
                var members = new List<string>();

                var query = g.V().HasLabel(EntGraphConstants.AccessCardVertexName)
                                .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                                .Has(EntGraphConstants.AccessConfigurationTypeName, accessConfigType);

                var results = await Submit<AccessCard>(query);

                foreach (var result in results)
                    if (result.Registry?.Split('|').Count() > 1)
                        members.Add(result.Registry.Split('|')[1]);

                return members;
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
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Email", email);

                var existingAccResult = await SubmitFirst<Account>(existingQuery);

                if (existingAccResult == null)
                {
                    var query = g.AddV(EntGraphConstants.AccountVertexName)
                        .Property("Email", email)
                        .Property(EntGraphConstants.RegistryName, registry);

                    existingAccResult = await SubmitFirst<Account>(query);
                }

                var existingPassportQuery = g.V(existingAccResult.ID)
                        .Out(EntGraphConstants.CarriesEdgeName)
                        .HasLabel(EntGraphConstants.PassportVertexName)
                        .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}")
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var existingPassportResult = await SubmitFirst<Passport>(existingPassportQuery);

                if (existingPassportResult == null)
                {
                    var passportQuery = g.AddV(EntGraphConstants.PassportVertexName)
                    .Property(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}")
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                    .Property("PasswordHash", password.ToMD5Hash())
                    .Property("IsActive", true);

                    existingPassportResult = await SubmitFirst<Passport>(passportQuery);

                    await ensureEdgeRelationships(g, existingAccResult.ID, existingPassportResult.ID,
                        edgeToCheckBuy: EntGraphConstants.CarriesEdgeName, edgesToCreate: new List<string>()
                        {
                                EntGraphConstants.CarriesEdgeName
                        });

                    status = Status.Success;
                }
                else
                    return Status.Conflict.Clone("Passport already exists.");

                if (!status)
                    return Status.GeneralError.Clone("There was an issue registering the current account.");

                return status;
            }, entApiKey);
        }

        public virtual async Task<string> RetrieveThirdPartyAccessToken(string entApiKey, string email, string key)
        {
            return await withG(async (client, g) =>
            {
                string isEncrypted = "false", tokenResult = String.Empty;

                var registry = email.Split('@')[1];

                if (!entApiKey.IsNullOrEmpty())
                {
                    var existingEntQuery = g.V()
                        .HasLabel(EntGraphConstants.AccountVertexName)
                        .Has(EntGraphConstants.RegistryName, registry)
                        .Has("Email", email)
                        .Out(EntGraphConstants.CarriesEdgeName)
                        .HasLabel(EntGraphConstants.PassportVertexName)
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                        .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}")
                        .Out(EntGraphConstants.OwnsEdgeName)
                        .HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
                        .Has(EntGraphConstants.RegistryName, email)
                        .Has("Key", key);

                    var entTptResult = await SubmitFirst<BusinessModel<Guid>>(existingEntQuery);

                    //return entTptResult?.Metadata["Token"].ToString();

                    if (entTptResult.Metadata.ContainsKey("Encrypt"))
                        isEncrypted = entTptResult?.Metadata["Encrypt"]?.ToString();

                    tokenResult = (isEncrypted == "true") ?
                        Encryption.Decrypt(entTptResult?.Metadata["Token"].ToString(), config["TOKEN-ENCODING-KEY"]) :
                        entTptResult?.Metadata["Token"].ToString();

                }
                else
                {
                    var existingAccQuery = g.V()
                        .HasLabel(EntGraphConstants.AccountVertexName)
                        .Has(EntGraphConstants.RegistryName, registry)
                        .Has("Email", email)
                        .Out(EntGraphConstants.OwnsEdgeName)
                        .HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
                        .Has(EntGraphConstants.RegistryName, email)
                        .Has("Key", key);

                    var accTptResult = await SubmitFirst<BusinessModel<Guid>>(existingAccQuery);

                    if (accTptResult.Metadata.ContainsKey("Encrypt"))
                        isEncrypted = accTptResult?.Metadata["Encrypt"]?.ToString();

                    tokenResult = (isEncrypted == "true") ?
                        Encryption.Decrypt(accTptResult?.Metadata["Token"].ToString(), config["TOKEN-ENCODING-KEY"]) :
                        accTptResult?.Metadata["Token"].ToString();

                }

                return tokenResult;
            }, entApiKey);
        }

        public virtual async Task<Status> SetThirdPartyAccessToken(string entApiKey, string email, string key, string token, string encrypt = "false")
        {
            return await withG(async (client, g) =>
            {
                var registry = email.Split('@')[1];

                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.AccountVertexName)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Email", email);

                if (!entApiKey.IsNullOrEmpty())
                {
                    existingQuery = existingQuery
                    .Out(EntGraphConstants.CarriesEdgeName)
                    .HasLabel(EntGraphConstants.PassportVertexName)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}");
                }

                existingQuery = existingQuery
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.ThirdPartyTokenVertexName)
                    .Has(EntGraphConstants.RegistryName, email)
                    .Has("Key", key);

                var tptResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

                var setQuery = tptResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.ThirdPartyTokenVertexName)
                        .Property(EntGraphConstants.RegistryName, email)
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                        .Property("Key", key);

                setQuery = setQuery.Property("Token", token)
                    .Property("Encrypt", encrypt);

                tptResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                if (!entApiKey.IsNullOrEmpty())
                {
                    var passQuery = g.V().HasLabel(EntGraphConstants.AccountVertexName)
                        .Has(EntGraphConstants.RegistryName, registry)
                        .Has("Email", email)
                        .Out(EntGraphConstants.CarriesEdgeName)
                        .HasLabel(EntGraphConstants.PassportVertexName)
                        .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}")
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

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
            }, entApiKey);
        }

        public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entApiKey, string username)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.AccessCardVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
                    .Has("AccessConfigurationType", accessCard.AccessConfigurationType);

                var acResult = await SubmitFirst<AccessCard>(existingQuery);

                var setQuery = acResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.AccessCardVertexName)
                        .Property(EntGraphConstants.RegistryName, $"{entApiKey}|{username}")
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
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
                        .Has(EntGraphConstants.RegistryName, entApiKey)
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

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
            }, entApiKey);
        }

        public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entApiKey)
        {
            return await withG(async (client, g) =>
            {
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.RelyingPartyVertexName)
                    .Has(EntGraphConstants.RegistryName, entApiKey)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                var rpResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

                var setQuery = rpResult != null ? existingQuery :
                    g.AddV(EntGraphConstants.RelyingPartyVertexName)
                        .Property(EntGraphConstants.RegistryName, entApiKey)
                        .Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

                setQuery = setQuery
                    .Property("AccessConfigurations", relyingParty.AccessConfigurations.ToJSON())
                    .Property("AccessRights", relyingParty.AccessRights.ToJSON())
                    .Property("DefaultAccessConfigurationType", relyingParty.DefaultAccessConfigurationType)
                    .Property("Providers", relyingParty.Providers.ToJSON());

                rpResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                if (rpResult != null)
                {
                    var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                        .Has(EntGraphConstants.RegistryName, entApiKey)
                        .Has("PrimaryAPIKey", entApiKey);

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
            }, entApiKey);
        }

        public virtual async Task<LicenseAccessToken> SetLicenseAccessToken(LicenseAccessToken token)
        {
            return await withG(async (client, g) =>
            {
                string username = token.UserName ?? token.Metadata["UserName"].ToString();

                // Verify user account exists
                var existingAccountQuery = g.V()
                            .HasLabel(EntGraphConstants.AccountVertexName)
                            .Has("Email", username);
                var accResult = await SubmitFirst<Account>(existingAccountQuery);

                // If not, return status of failure
                if (accResult == null) return null;

                // Check for existing token
                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                    .Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.UserName}")
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey);

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
                        expDate = System.DateTime.Now.AddDays(Convert.ToInt16(token.Metadata["TrialPeriodDays"]));
                    }

                    var setQuery =
                        g.V().HasLabel(EntGraphConstants.LicenseAccessTokenVertexName)
                        .Has(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.UserName}")
                        .Has(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
                        .Property("AccessStartDate", accDate)
                        .Property("ExpirationDate", expDate)
                        .AttachMetadataProperties<LicenseAccessToken>(token);

                    var updateResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

                    tokResult = updateResult.JSONConvert<LicenseAccessToken>();
                }
                else
                {
                    // If not, create the license access token 
                    var setQuery =
                        g.AddV(EntGraphConstants.LicenseAccessTokenVertexName)
                            .Property(EntGraphConstants.RegistryName, $"{token.EnterpriseAPIKey}|{token.UserName}")
                            .Property(EntGraphConstants.EnterpriseAPIKeyName, token.EnterpriseAPIKey)
                            .Property("AccessStartDate", System.DateTime.Now)
                            .Property("ExpirationDate", System.DateTime.Now.AddDays(Convert.ToInt16(token.Metadata["TrialPeriodDays"].ToString())))
                            .AttachMetadataProperties<LicenseAccessToken>(token);

                    tokResult = await SubmitFirst<LicenseAccessToken>(setQuery);

                }
                return tokResult;
            }, token.EnterpriseAPIKey);
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
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Out(EntGraphConstants.CarriesEdgeName)
                    .HasLabel(EntGraphConstants.PassportVertexName)
                    .Has("IsActive", true)
                    .Has("PasswordHash", password.ToMD5Hash())
                    .Has(EntGraphConstants.RegistryName, $"{entApiKey}|{registry}");

                var accResult = await SubmitFirst<Passport>(existingQuery);

                if (accResult != null)
                    status = Status.Success;
                else
                    status = Status.Unauthorized;

                return status;
            }, entApiKey);
        }


        #endregion

        #region Helpers
        #endregion
    }
}
