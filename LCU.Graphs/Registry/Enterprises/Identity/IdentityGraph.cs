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

                await ensureEdgeRelationship<Carries>(account.ID, passport.ID);
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
            {
                tpiQuery = tpiQuery
                    .Out<Carries>()
                    .OfType<Passport>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == $"{entLookup}|{registry}");
            }

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

                var parentId = account.ID;

                if (!entLookup.IsNullOrEmpty())
                {
                    var passport = await GetPassport(email, entLookup);

                    parentId = passport.ID;
                }

                await ensureEdgeRelationship<Owns>(parentId, tpi.ID);
            }
            else
            {
                tpi.Encrypt = !tokenEncodingKey.IsNullOrEmpty();

                tpi.Token = token;

                tpi = await g.V<ThirdPartyToken>(tpi.ID)
                    .Update(tpi)
                    .FirstOrDefaultAsync();
            }

            return Status.Success;
        }

        public virtual async Task<AccessCard> SaveAccessCard(AccessCard accessCard, string entLookup, string username)
        {
            var account = await GetAccount(username);

            var existingAccessCard = await GetAccessCard(entLookup, username, accessCard.AccessConfigurationType);

            accessCard.EnterpriseLookup = entLookup;

            accessCard.Registry = $"{entLookup}|{username}";

            if (existingAccessCard == null)
            {
                if (accessCard.ID.IsEmpty())
                    accessCard.ID = Guid.NewGuid();

                accessCard = await g.AddV(accessCard).FirstOrDefaultAsync();

                await ensureEdgeRelationship<Carries>(account.ID, accessCard.ID);
            }
            else
            {
                accessCard.ID = existingAccessCard.ID;

                accessCard = await g.V<AccessCard>(accessCard.ID)
                    .Update(accessCard)
                    .FirstOrDefaultAsync();
            }

            if (accessCard != null)
            {
                var rp = await GetRelyingParty(entLookup);

                if (rp != null)
                {
                    await ensureEdgeRelationship<Provides>(rp.ID, accessCard.ID);

                    await ensureEdgeRelationship<Consumes>(account.ID, accessCard.ID);
                }
            }

            return accessCard;
        }

        public virtual async Task<RelyingParty> SaveRelyingParty(RelyingParty relyingParty, string entLookup)
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

                await ensureEdgeRelationship<Owns>(ent.ID, relyingParty.ID);
            }
            else
            {
                relyingParty.ID = existingRP.ID;

                relyingParty = await g.V<RelyingParty>(relyingParty.ID)
                    .Update(relyingParty)
                    .FirstOrDefaultAsync();
            }

            return relyingParty;
        }

        public virtual async Task<LicenseAccessToken> SetLicenseAccessToken(string entLookup, string username, LicenseAccessToken token)
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
                token.ID = existingLAT.ID;

                token = await g.V<LicenseAccessToken>(token.ID)
                    .Update(token)
                    .FirstOrDefaultAsync();
            }

            return token;
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
