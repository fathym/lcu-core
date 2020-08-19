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

        protected readonly string username = "testuser@fathym.com";

        protected readonly string password = "somepassword";

        protected readonly string tokenKey = "TEST_THIRD_PARTY_TOKEN";

        protected readonly string tokenValue = "test token value";

        protected readonly string accessConfigType = "LCU";
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
        public async Task CreateThirdPartyTokenRemove()
        {
            var expected = createThirdPartyToken(mainEnt.EnterpriseLookup, username);

            var status = await identityGraph.SetThirdPartyAccessToken(mainEnt.EnterpriseLookup, username, expected.Key, expected.Token);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            var actual = await identityGraph.RetrieveThirdPartyAccessToken(mainEnt.EnterpriseLookup, username, expected.Key);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Token, actual);
        }

        [TestMethod]
        public async Task CreateAccessCardRemove()
        {
            var expected = createAccessCard(mainEnt.EnterpriseLookup);

            var actual = await identityGraph.SaveAccessCard(expected, mainEnt.EnterpriseLookup, username);

            // TODO: Use reflection to write an object equivalency checker and add it to LCU.Testing
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.AccessConfigurationType, actual.AccessConfigurationType);
            Assert.AreEqual(expected.Registry, actual.Registry);

            var status = await identityGraph.DeleteAccessCard(mainEnt.EnterpriseLookup, username, accessConfigType);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);
        }

        [TestMethod]
        public async Task CreateRelyingPartyRemove()
        {
            var expected = createRelyingParty(mainEnt.EnterpriseLookup);

            var actual = await identityGraph.SaveRelyingParty(expected, mainEnt.EnterpriseLookup);

            // TODO: Use reflection to write an object equivalency checker and add it to LCU.Testing
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.Registry, actual.Registry);
            Assert.AreEqual(expected.DefaultAccessConfigurationType, actual.DefaultAccessConfigurationType);

            var status = await identityGraph.DeleteRelyingParty(mainEnt.EnterpriseLookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);
        }

        [TestMethod]
        public async Task CreateLicenseAccessTokenRemove()
        {
            var expected = createLicenseAccessToken(mainEnt.EnterpriseLookup, username);

            var actual = await identityGraph.SetLicenseAccessToken(mainEnt.EnterpriseLookup, username, expected);

            // TODO: Use reflection to write an object equivalency checker and add it to LCU.Testing
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.Registry, actual.Registry);
            Assert.AreEqual(expected.IsLocked, actual.IsLocked);
            Assert.AreEqual(expected.IsReset, actual.IsReset);
            Assert.AreEqual(expected.Label, actual.Label);
            Assert.AreEqual(expected.Lookup, actual.Lookup);
            Assert.AreEqual(expected.TrialPeriodDays, actual.TrialPeriodDays);
            Assert.AreEqual(expected.AccessStartDate, actual.AccessStartDate);
            Assert.AreEqual(expected.ExpirationDate, actual.ExpirationDate);

            var metadata = actual.Metadata?.JSONConvert<Dictionary<string, JToken>>();

            Assert.IsNotNull(metadata);
            Assert.AreEqual(expected.Metadata["PlanGroup"], metadata["PlanGroup"]);
            Assert.AreEqual(expected.Metadata["Priority"], metadata["Priority"]);
            Assert.AreEqual(expected.Metadata["Price"], metadata["Price"]);
            Assert.AreEqual(expected.Metadata["DataApps"], metadata["DataApps"]);
            Assert.AreEqual(expected.Metadata["DataFlows"], metadata["DataFlows"]);

            var status = await identityGraph.DeleteLicenseAccessToken(mainEnt.EnterpriseLookup, username, expected.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);
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

        protected virtual AccessCard createAccessCard(string entId)
        {
            return new AccessCard()
            {
                AccessConfigurationType = accessConfigType,
                ExcludeAccessRights = new List<string>().ToArray(),
                IncludeAccessRights = new List<string>().ToArray(),
                ID = Guid.NewGuid(),
                Registry = entId,
                EnterpriseLookup = entId,
            };
        }

        protected virtual RelyingParty createRelyingParty(string entId)
        {
            return new RelyingParty()
            {
                EnterpriseLookup = entId,
                Registry = entId,
                ID = Guid.NewGuid(),
                DefaultAccessConfigurationType = accessConfigType
            };
        }
        #endregion
    }
}
