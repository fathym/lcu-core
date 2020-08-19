using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Testing.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Identity
{
    [TestClass]
    public class IdentityGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string accessRight = "LCU.Test.Everything";

        protected readonly IdentityGraph identityGraph;

        protected readonly string license = "lcu";

        protected readonly string domain = "testdomain.fathym.com";

        protected readonly string password = "somepassword";

        protected readonly string tokenKey = "TEST_THIRD_PARTY_TOKEN";

        protected readonly string tokenValue = "test token value";
        #endregion

        #region Constructors
        public IdentityGraphTests()
            : base()
        {
            identityGraph = new IdentityGraph(graphConfig, createLogger<IdentityGraph>());
        }
        #endregion

        #region Life Cycle
        [TestCleanup]
        public virtual async Task Cleanup()
        {
            await cleanupEnterprises();
        }

        [TestInitialize]
        public virtual async Task Initialize()
        {
            await setupMainEnt(entGraph, null, null, identityGraph);
        }
        #endregion

        #region API Methods
        [TestMethod]
        public async Task RetrieveAccessCards()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task RetrieveAccounts()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task RetrievePassports()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task RetrieveRelyingParty()
        {

            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task RetrieveLicenseAccessTokens()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task CreateThirdPartyTokenRemove()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task CreateAccessCardRemove()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task CreateRelyingPartyRemove()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task TestAuthorization()
        {
            throw new NotImplementedException("Not implemented");
        }
        #endregion

        //TODO: this content can be encapsulated in a snapshot we append as a json/resource file
        #region Helpers
        protected virtual Passport createPassport(string entId)
        {
            return new Passport()
            {
                EnterpriseLookup = entId,
                IsActive = true,
                ID = Guid.NewGuid(),
                Label = "Passport",
                Registry = $"{entId}|{domain}",
                PasswordHash = password.ToMD5Hash(),
                ProviderID = Guid.NewGuid().ToString()
            };
        }

        protected virtual LicenseAccessToken createLicenseAccessToken(string entId, string username)
        {
            var now = System.DateTime.Now;

            return new LicenseAccessToken()
            {
                EnterpriseLookup = entId,
                Username = username,
                AccessStartDate = now,
                ExpirationDate = now.AddDays(7.0),
                ID = Guid.NewGuid(),
                IsLocked = false,
                IsReset = false,
                Label = "LicenseAccessToken",
                Lookup = license,
                Registry = $"{entId}|{username}",
                TrialPeriodDays = 7,
                Metadata = new Dictionary<string, JToken>()
                        {
                            { "PlanGroup", "trial" },
                            { "Priority", "20" },
                            { "Price", "0" },
                            { "DataApps", "5" },
                            { "DataFlows", "5" }
                        }

            };
        }

        protected virtual ThirdPartyToken createThirdPartyToken(string entId, string username)
        {
            return new ThirdPartyToken()
            {
                EnterpriseLookup = entId,
                ID = Guid.NewGuid(),
                Label = "ThirdPartyToken",
                Registry = username,
                Key = tokenKey,
                Encrypt = false,
                Token = tokenValue
            };
        }

        protected virtual Account createAccount(string entId, string username)
        {
            return new Account()
            {
                EnterpriseLookup = entId,
                Email = username,
                ID = Guid.NewGuid(),
                Label = "Account",
                Registry = $"{domain}"
            };
        }

        #endregion
    }
}
