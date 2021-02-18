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
            return await withCommonGraphBoundary(async () =>
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
            });
        }

        public virtual async Task<Status> DeleteRelyingParty(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
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
            });
        }

        public virtual async Task<Status> DeleteLicenseAccessToken(string entLookup, string username, string lookup)
        {
            return await withCommonGraphBoundary(async () =>
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
            });
        }

        public virtual async Task<Status> DeletePassport(string entLookup, string username)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var passport = await GetPassport(username, entLookup);

                if (passport != null)
                {
                    await g.V<Passport>(passport.ID)
                        .Where(e => e.EnterpriseLookup == entLookup)
                        .Drop();

                    return Status.Success;
                }
                else
                {
                    return Status.GeneralError.Clone("Unable to locate user passport");
                }
            });
        }

        public virtual async Task<Status> Exists(string email, string entLookup = null)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var registry = email.Split('@')[1];

                var existing = await GetPassport(email, entLookup);

                if (existing != null)
                    return Status.Success;
                else
                    return Status.NotLocated;
            });
        }

        public virtual async Task<Account> GetAccount(string email)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var registry = email.Split('@')[1];

                return await g.V<Account>()
                      .Where(e => e.Registry == registry)
                      .Where(e => e.Email == email)
                      .FirstOrDefaultAsync();
            });
        }

        public virtual async Task<Passport> GetPassport(string email, string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var registry = email.Split('@')[1];

                var passport = await g.V<Account>()
                    .Where(e => e.Registry == registry)
                    .Where(e => e.Email == email)
                    .Out<Carries>()
                    .OfType<Passport>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.IsActive)
                    .FirstOrDefaultAsync();

                if (passport?.EnterpriseLookup != entLookup)
                    passport = null;

                return passport;
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

        public virtual async Task<AccessCard> GetAccessCard(string entLookup, string username, string accessConfigType)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var registry = username.Split('@')[1];

                var accessCard = await g.V<AccessCard>()
                      .Where(e => e.EnterpriseLookup == entLookup)
                      .Where(e => e.Registry == $"{entLookup}|{username}")
                      .Where(e => e.AccessConfigurationType == accessConfigType)
                      .FirstOrDefaultAsync();

                return accessCard;
            });
        }

        public virtual async Task<RelyingParty> GetRelyingParty(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var rp = await g.V<RelyingParty>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == entLookup)
                  .FirstOrDefaultAsync();

                return rp;
            });
        }

        public virtual async Task<LicenseAccessToken> GetLicenseAccessToken(string entLookup, string username, string lookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var lat = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .Where(e => e.Lookup == lookup)
                  .FirstOrDefaultAsync();

                return lat;
            });
        }

        //public virtual async Task<Dictionary<string, LicenseAccessToken>> GetLicenseAccessTokenForUsers(string entLookup, List<string> usernames, string lookup)
        //{
        //    return await withCommonGraphBoundary(async () =>
        //    {

        //        var lat = await g.V<LicenseAccessToken>()
        //          .Where(e => e.EnterpriseLookup == entLookup)
        //          .Where(e => e.Registry == $"{entLookup}|{username}")
        //          .Where(e => e.Lookup == lookup)
        //          .FirstOrDefaultAsync();

        //        return lat;
        //    });
        //}

        public virtual async Task<List<Account>> ListAccountsByOrg(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var accounts = await g.V<Passport>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .InE<Carries>()
                  .OutV<Account>()
                  .ToListAsync();

                return accounts;
            });
        }

        public virtual async Task<List<AccessCard>> ListAccessCards(string entLookup, string username)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var accessCards = await g.V<AccessCard>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .ToListAsync();

                return accessCards;
            });
        }

        public virtual async Task<List<string>> ListAdmins(string entLookup)
        {
            return await ListMembersWithAccessConfigType(entLookup, "LCU");
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokens(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var lats = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .ToListAsync();

                return lats;
            });
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokensByLookup(string entLookup, string lookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var lats = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Lookup == lookup)
                  .ToListAsync();

                return lats;
            });
        }

        public virtual async Task<List<LicenseAccessToken>> ListLicenseAccessTokensByUser(string entLookup, string username)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var lats = await g.V<LicenseAccessToken>()
                  .Where(e => e.EnterpriseLookup == entLookup)
                  .Where(e => e.Registry == $"{entLookup}|{username}")
                  .ToListAsync();

                return lats;
            });
        }

        public virtual async Task<List<string>> ListMembersWithAccessConfigType(string entLookup, string accessConfigType)
        {
            return await withCommonGraphBoundary(async () =>
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
            });
        }

        public virtual async Task<Status> Register(string entLookup, string email, string password, string providerId)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var registry = email.Split('@')[1];

                var account = await GetAccount(email);

                if (account == null)
                    account = await g.AddV(new Account()
                    {
                        ID = Guid.NewGuid(),
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

                    await EnsureEdgeRelationship<Carries>(account.ID, passport.ID);
                }
                else
                {
                    passport.PasswordHash = password.ToMD5Hash();

                    passport.ProviderID = providerId;

                    passport = await g.V<Passport>(passport.ID)
                        .Update(passport)
                        .FirstOrDefaultAsync();
                }

                return Status.Success;
            });
        }

        public virtual async Task<string> RetrieveThirdPartyAccessToken(string entLookup, string email, string key)
        {
            return await withCommonGraphBoundary(async () =>
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
                    .Where(e => e.Key == key)
                    .FirstOrDefaultAsync();

                return tpi?.Token;
            });
        }

        public virtual async Task<Status> SetThirdPartyAccessToken(string entLookup, string email, string key, string token)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var account = await GetAccount(email);

                var accRegistry = email.Split('@')[1];

                IVertexGremlinQueryBase tpiQuery = g.V<Account>(account.ID)
                    .Where(e => e.Registry == accRegistry)
                    .Where(e => e.Email == email);

                if (!entLookup.IsNullOrEmpty())
                {
                    tpiQuery = tpiQuery
                        .Out<Carries>()
                        .OfType<Passport>()
                        .Where(e => e.EnterpriseLookup == entLookup)
                        .Where(e => e.Registry == $"{entLookup}|{accRegistry}");
                }

                var tpi = await tpiQuery
                    .Out<Owns>()
                    .OfType<ThirdPartyToken>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == email)
                    .Where(e => e.Key == key)
                    .FirstOrDefaultAsync();

                if (tpi == null)
                {
                    tpi = new ThirdPartyToken()
                    {
                        ID = Guid.NewGuid(),
                        EnterpriseLookup = entLookup,
                        Registry = email,
                        Key = key,
                        Token = token
                    };

                    tpi = await g.AddV(tpi).FirstOrDefaultAsync();

                    var parentId = account.ID;

                    if (!entLookup.IsNullOrEmpty())
                    {
                        var passport = await GetPassport(email, entLookup);

                        parentId = passport.ID;
                    }

                    await EnsureEdgeRelationship<Owns>(parentId, tpi.ID);
                }
                else
                {
                    tpi.Token = token;

                    tpi = await g.V<ThirdPartyToken>(tpi.ID)
                        .Update(tpi)
                        .FirstOrDefaultAsync();
                }

                return Status.Success;
            });
        }

        public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entLookup, string username)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var account = await GetAccount(username);

                var existingAccessCard = await GetAccessCard(entLookup, username, accessCard.AccessConfigurationType);

                accessCard.EnterpriseLookup = entLookup;

                accessCard.Registry = $"{entLookup}|{username}";

                accessCard.LastAccess = buildAudit(by: username, description: $"Last accessed by {entLookup}");

                if (existingAccessCard == null)
                {
                    if (accessCard.ID.IsEmpty())
                        accessCard.ID = Guid.NewGuid();

                    accessCard.FirstAccess = buildAudit(by: username, description: $"First accessed by {entLookup}");

                    accessCard = await g.AddV(accessCard).FirstOrDefaultAsync();

                    await EnsureEdgeRelationship<Carries>(account.ID, accessCard.ID);
                }
                else
                {
                    accessCard = await g.V<AccessCard>(existingAccessCard.ID)
                        .Update(accessCard)
                        .FirstOrDefaultAsync();
                }

                if (accessCard != null)
                {
                    var rp = await GetRelyingParty(entLookup);

                    if (rp != null)
                    {
                        await EnsureEdgeRelationship<Provides>(rp.ID, accessCard.ID);

                        await EnsureEdgeRelationship<Consumes>(account.ID, accessCard.ID);
                    }
                }

                return accessCard;
            });
        }

        public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var existingRP = await GetRelyingParty(entLookup);

                relyingParty.EnterpriseLookup = entLookup;

                relyingParty.Registry = entLookup;

                if (existingRP == null)
                {
                    if (relyingParty.ID.IsEmpty())
                        relyingParty.ID = Guid.NewGuid();

                    relyingParty = await g.AddV(relyingParty).FirstOrDefaultAsync();

                    var ent = await g.V<Enterprise>()
                        .Where(e => e.EnterpriseLookup == entLookup)
                        .Where(e => e.Registry == entLookup)
                        .FirstOrDefaultAsync();

                    await EnsureEdgeRelationship<Owns>(ent.ID, relyingParty.ID);
                }
                else
                {
                    relyingParty = await g.V<RelyingParty>(existingRP.ID)
                        .Update(relyingParty)
                        .FirstOrDefaultAsync();
                }

                return relyingParty;
            });
        }

        public virtual async Task<LicenseAccessToken> SetLicenseAccessToken(string entLookup, string username, LicenseAccessToken token)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var existingLAT = await GetLicenseAccessToken(entLookup, username, token.Lookup);

                token.EnterpriseLookup = entLookup;

                token.Registry = $"{entLookup}|{username}";

                if (token.IsLocked)
                    token.ExpirationDate = System.DateTime.Now;

                if (token.IsReset)
                {
                    token.AccessStartDate = System.DateTime.Now;

                    if (token.TrialPeriodDays > 0)
                        token.ExpirationDate = DateTime.Now.AddDays(token.TrialPeriodDays);
                }

                if (existingLAT == null)
                {
                    if (token.ID.IsEmpty())
                        token.ID = Guid.NewGuid();

                    token = await g.AddV(token).FirstOrDefaultAsync();
                }
                else
                {
                    token = await g.V<LicenseAccessToken>(existingLAT.ID)
                        .Update(token)
                        .FirstOrDefaultAsync();
                }

                return token;
            });
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
