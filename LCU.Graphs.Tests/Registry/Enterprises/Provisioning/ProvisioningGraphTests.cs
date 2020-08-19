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
        protected readonly string testActivity = "qatest";

        protected readonly string testLookup = "test-lookup-int";

        protected readonly string testEnvLookup = "env-test-lookup-int";

        protected readonly string testOrgLookup = "org-test-lookup-int";

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
            var expected = createEnvironment(mainEnt.EnterpriseLookup);

            var actual = await prvGraph.SaveEnvironment(mainEnt.EnterpriseLookup, expected);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.Label, actual.Label);
            Assert.AreEqual(expected.Lookup, actual.Lookup);
            Assert.AreEqual(expected.Registry, actual.Registry);
            Assert.AreEqual(expected.Registry, actual.Registry);

            var metadata = actual.Metadata?.JSONConvert< Dictionary<string, JToken>>();

            Assert.IsNotNull(metadata);
            Assert.AreEqual(expected.Metadata["Name"], metadata["Name"]);

            var status = await prvGraph.RemoveEnvironment(mainEnt.EnterpriseLookup, expected.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

        }

        [TestMethod]
        public async Task CreateEnvironmentSettingsRemove()
        {
            var expected = createEnvironmentSettings(mainEnt.EnterpriseLookup);

            var actual = await prvGraph.SaveEnvironmentSettings(mainEnt.EnterpriseLookup, testEnvLookup, expected);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.Label, actual.Label);
            Assert.AreEqual(expected.Registry, actual.Registry);
            
            var metadata = actual.Settings?.JSONConvert<Dictionary<string, JToken>>();

            Assert.IsNotNull(metadata);
            Assert.AreEqual(expected.Metadata["Registry"], metadata["Registry"]);
            Assert.AreEqual(expected.Metadata["EnterpriseAPIKey"], metadata["EnterpriseAPIKey"]);
            Assert.AreEqual(expected.Metadata["AzureTenantID"], metadata["AzureTenantID"]);
            Assert.AreEqual(expected.Metadata["AzureSubID"], metadata["AzureSubID"]);
            Assert.AreEqual(expected.Metadata["AzureAppID"], metadata["AzureAppID"]);
            Assert.AreEqual(expected.Metadata["AzureAppAuthkey"], metadata["AzureAppAuthkey"]);
            Assert.AreEqual(expected.Metadata["EnvironmentLookup"], metadata["EnvironmentLookup"]);
            Assert.AreEqual(expected.Metadata["OrganizationLookup"], metadata["OrganizationLookup"]);
            Assert.AreEqual(expected.Metadata["InfrastructureRepoName"], metadata["InfrastructureRepoName"]);
            Assert.AreEqual(expected.Metadata["AzureRegion"], metadata["AzureRegion"]);
            Assert.AreEqual(expected.Metadata["AzureLocation"], metadata["AzureLocation"]);

            var status = await prvGraph.RemoveEnvironmentSettings(mainEnt.EnterpriseLookup, expected.Metadata["EnvironmentLookup"].ToString());

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

        }

        // TODO: Not even sure about this one
        [Ignore]
        public async Task CreateSourceControlRemove()
        {
            var expected = createSourceControl(mainEnt.EnterpriseLookup);


            var actual = await prvGraph.SaveSourceControl(mainEnt.EnterpriseLookup, testEnvLookup, expected);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.EnterpriseLookup, actual.EnterpriseLookup);
            Assert.AreEqual(expected.Label, actual.Label);
            Assert.AreEqual(expected.Registry, actual.Registry);
            Assert.AreEqual(expected.Repository, actual.Repository);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Organization, actual.Organization);
        }
        #endregion

        #region Helpers
        protected virtual LCUEnvironment createEnvironment(string entId)
        {
            return new LCUEnvironment()
            {
                EnterpriseLookup = entId,
                ID = Guid.NewGuid(),
                Label = "Environment",
                Lookup = $"{testLookup}",
                Registry = entId,
                Metadata = new Dictionary<string, JToken>()
                        {
                            { "Name", "Test Lookup Int" }
                        }
            };
        }

        protected virtual LCUEnvironmentSettings createEnvironmentSettings(string entId)
        {

            return new LCUEnvironmentSettings()
            {
                EnterpriseLookup = entId,
                ID = Guid.NewGuid(),
                Label = "EnvironmentSettings",
                Registry = entId,
                Settings = new MetadataModel()
                {
                    Metadata = new Dictionary<string, JToken>()
                        {
                            { "Registry", entId },
                            { "EnterpriseAPIKey", entId },
                            { "AzureTenantID", Guid.NewGuid() },
                            { "AzureSubID", Guid.NewGuid() },
                            { "AzureAppID", Guid.NewGuid() },
                            { "AzureAppAuthkey", String.Empty },
                            { "EnvironmentLookup", testEnvLookup },
                            { "OrganizationLookup", testOrgLookup },
                            { "InfrastructureRepoName", "infra-repo" },
                            { "AzureRegion", "westus" },
                            { "AzureLocation", "West US" }
                        }
                }
            };
        }

        // TODO: Cannot find a real SourceControl vertex, do we still use this?
        protected virtual SourceControl createSourceControl(string entId)
        {
            return new SourceControl()
            {
                EnterpriseLookup = entId,
                ID = Guid.NewGuid(),
                Label = "SourceControl",
                Name = "Test Repo Inf",
                Organization = testOrgLookup,
                Registry = entId,
                Repository = testRepo
            };
        }
        #endregion
    }
}
