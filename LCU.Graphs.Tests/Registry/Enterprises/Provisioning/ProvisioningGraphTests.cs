using Fathym;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using LCU.Testing.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Provisioning
{
    [TestClass]
    public class ProvisioningGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string testLookup = "test-lookup-int";

        protected readonly string testRepo = "inf-repo-test";

        protected readonly ProvisioningGraph prvGraph;
        #endregion

        #region Constructors
        public ProvisioningGraphTests()
            : base()
        {
            prvGraph = new ProvisioningGraph(graphConfig, createLogger<ProvisioningGraph>());
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
            await setupMainEnt(entGraph, appGraph: null, prvGraph: prvGraph);
        }
        #endregion
        
        #region API Methods
        [TestMethod]
        public async Task CreateEnvironmentRemove()
        {
            var expected = createEnvironment();

            var actual = await prvGraph.SaveEnvironment(mainEnt.EnterpriseLookup, expected);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Lookup, actual.Lookup);

            var status = await prvGraph.RemoveEnvironment(mainEnt.EnterpriseLookup, expected.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

        }

        [TestMethod]
        public async Task CreateEnvironmentSettingsRemove()
        {
            var expected = createEnvironmentSettings();

            var actual = await prvGraph.SaveEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup, expected);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Settings);
            Assert.AreEqual(expected.Settings.Metadata["AzureTenantID"].ToString(), actual.Settings.Metadata["AzureTenantID"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["AzureSubID"].ToString(), actual.Settings.Metadata["AzureSubID"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["AzureAppID"].ToString(), actual.Settings.Metadata["AzureAppID"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["AzureAppAuthkey"].ToString(), actual.Settings.Metadata["AzureAppAuthkey"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["EnvironmentLookup"].ToString(), actual.Settings.Metadata["EnvironmentLookup"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["OrganizationLookup"].ToString(), actual.Settings.Metadata["OrganizationLookup"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["InfrastructureRepoName"].ToString(), actual.Settings.Metadata["InfrastructureRepoName"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["AzureRegion"].ToString(), actual.Settings.Metadata["AzureRegion"].ToString());
            Assert.AreEqual(expected.Settings.Metadata["AzureLocation"].ToString(), actual.Settings.Metadata["AzureLocation"].ToString());

            var status = await prvGraph.RemoveEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);
        }

        // TODO: Not even sure about this one
        [Ignore]
        public async Task CreateSourceControlRemove()
        {
            var expected = createSourceControl();


            var actual = await prvGraph.SaveSourceControl(mainEnt.EnterpriseLookup, mainEnv.Lookup, expected);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Repository, actual.Repository);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Organization, actual.Organization);
        }
        #endregion

        #region Helpers
        protected virtual LCUEnvironment createEnvironment()
        {
            return new LCUEnvironment()
            {
                Lookup = $"{testLookup}"
            };
        }

        protected virtual LCUEnvironmentSettings createEnvironmentSettings()
        {

            return new LCUEnvironmentSettings()
            {
                Settings = new MetadataModel()
                {
                    Metadata = new Dictionary<string, JToken>()
                    {
                        { "AzureTenantID", Guid.NewGuid() },
                        { "AzureSubID", Guid.NewGuid() },
                        { "AzureAppID", Guid.NewGuid() },
                        { "AzureAppAuthkey", String.Empty },
                        { "EnvironmentLookup", mainEnv.Lookup },
                        { "OrganizationLookup", orgLookup },
                        { "InfrastructureRepoName", "infra-repo" },
                        { "AzureRegion", "westus" },
                        { "AzureLocation", "West US" }
                    }
                }
            };
        }

        // TODO: Cannot find a real SourceControl vertex, do we still use this?
        protected virtual SourceControl createSourceControl()
        {
            return new SourceControl()
            {
                Name = "Test Repo Inf",
                Organization = orgLookup,
                Repository = testRepo
            };
        }
        #endregion
    }
}
