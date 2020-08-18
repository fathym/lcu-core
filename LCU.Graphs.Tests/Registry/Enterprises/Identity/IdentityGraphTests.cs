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

        #region Helpers

        #endregion
    }
}
