using Fathym;
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

            var status = await identityGraph.SetThirdPartyAccessToken(null, username, expected.Key, expected.Token);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            var actual = await identityGraph.RetrieveThirdPartyAccessToken(null, username, expected.Key);

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
            Assert.AreEqual(expected.IsLocked, actual.IsLocked);
            Assert.AreEqual(expected.IsReset, actual.IsReset);
            Assert.AreEqual(expected.Lookup, actual.Lookup);
            Assert.AreEqual(expected.TrialPeriodDays, actual.TrialPeriodDays);
            Assert.AreEqual(expected.AccessStartDate.ToString("yyyy/MM/dd HH:mm:ss"), actual.AccessStartDate.ToString("yyyy/MM/dd HH:mm:ss"));
            Assert.AreEqual(expected.ExpirationDate.ToString("yyyy/MM/dd HH:mm:ss"), actual.ExpirationDate.ToString("yyyy/MM/dd HH:mm:ss"));

            Assert.IsNotNull(actual.Details);
            Assert.AreEqual(expected.Details.Metadata["PlanGroup"].ToString(), actual.Details.Metadata["PlanGroup"].ToString());
            Assert.AreEqual(expected.Details.Metadata["Priority"].ToString(), actual.Details.Metadata["Priority"].ToString());
            Assert.AreEqual(expected.Details.Metadata["Price"].ToString(), actual.Details.Metadata["Price"].ToString());
            Assert.AreEqual(expected.Details.Metadata["DataApps"].ToString(), actual.Details.Metadata["DataApps"].ToString());
            Assert.AreEqual(expected.Details.Metadata["DataFlows"].ToString(), actual.Details.Metadata["DataFlows"].ToString());

            var status = await identityGraph.DeleteLicenseAccessToken(mainEnt.EnterpriseLookup, username, expected.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);
        }

        //[TestMethod]
        //public async Task TestAuthorization()
        //{
        //    throw new NotImplementedException("Not implemented");
        //}
        #endregion

        //TODO: this content can be encapsulated in a snapshot we append as a json/resource file
        #region Helpers
        protected virtual Passport createPassport(string entId)
        {
            return new Passport()
            {
                IsActive = true,
                PasswordHash = password.ToMD5Hash(),
                ProviderID = Guid.NewGuid().ToString()
            };
        }

        protected virtual LicenseAccessToken createLicenseAccessToken(string entId, string username)
        {
            var now = System.DateTime.UtcNow;

            return new LicenseAccessToken()
            {
                Username = username,
                AccessStartDate = now,
                ExpirationDate = now.AddDays(7.0),
                IsLocked = false,
                IsReset = false,
                Lookup = license,
                TrialPeriodDays = 7,
                Details = new
                {
                    PlanGroup = "trial",
                    Priority = 20,
                    Price = 0,
                    DataApps = 5,
                    DataFlows = 5
                }.JSONConvert<MetadataModel>()

            };
        }

        protected virtual ThirdPartyToken createThirdPartyToken(string entId, string username)
        {
            return new ThirdPartyToken()
            {
                Key = tokenKey,
                Encrypt = false,
                Token = tokenValue
            };
        }

        protected virtual Account createAccount(string entId, string username)
        {
            return new Account()
            {
                Email = username,
            };
        }

        protected virtual AccessCard createAccessCard(string entId)
        {
            return new AccessCard()
            {
                AccessConfigurationType = accessConfigType,
                ExcludeAccessRights = new List<string>().ToArray(),
                IncludeAccessRights = new List<string>().ToArray()
            };
        }

        protected virtual RelyingParty createRelyingParty(string entId)
        {
            //	TODO:  How to power this by open source repo config, and enable white labeling users to define their own fork
            var nideAccessRight = new AccessRight()
            {
                Lookup = "LCU.NapkinIDE.AllAccess",
                Name = "LCU Napkin IDE - All Access",
                Description = "Represents complete access to the enterprise Napkin IDE instance."
            };

            var nideStakeHolderAccessRight = new AccessRight()
            {
                Lookup = "LCU.NapkinIDE.StakeHolder",
                Name = "LCU Napkin IDE - Stake Holder",
                Description = "Represents stake holder access to the enterprise Napkin IDE instance."
            };

            var initialAccessConfig = new AccessConfiguration()
            {
                Type = "LCU",
                AcceptedProviderIDs = Array.Empty<Guid>(),
                AccessRights = new string[]
                {
                    nideAccessRight.Lookup,
                    nideStakeHolderAccessRight.Lookup
                }
            };

            return new RelyingParty()
            {
                AccessConfigurations = new AccessConfiguration[] { initialAccessConfig },
                AccessRights = new AccessRight[] { nideAccessRight, nideStakeHolderAccessRight },
                DefaultAccessConfigurationType = "LCU",
                Providers = Array.Empty<Provider>(),
                //	TODO:  Implement providers concept throughout... not needed for MVP, let's craft a story
            };
        }
        #endregion
    }
}
