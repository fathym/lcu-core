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
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task CreateEnvironmentSettingsRemove()
        {
            throw new NotImplementedException("Not implemented");
        }

        [TestMethod]
        public async Task CreateSourceControlRemove()
        {
            throw new NotImplementedException("Not implemented");
        }
        //[TestMethod]
        //public async Task SaveListRemoveActivity()
        //{
        //    var expected = new IDEActivity()
        //    {
        //        Icon = "dashboard",
        //        Lookup = testActivity,
        //        Sections = new[] { "first", "second" },
        //        Title = "Dashboard"
        //    };

        //    var activity = await ideGraph.SaveActivity(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, expected);

        //    Assert.IsNotNull(activity);
        //    Assert.AreNotEqual(Guid.Empty, activity.ID);
        //    Assert.AreEqual(expected.EnterpriseLookup, activity.EnterpriseLookup);
        //    Assert.AreEqual(expected.Icon, activity.Icon);
        //    Assert.AreEqual(expected.Lookup, activity.Lookup);
        //    Assert.AreEqual(expected.Registry, activity.Registry);
        //    Assert.AreEqual(expected.Title, activity.Title);
        //    Assert.IsFalse(activity.Sections.IsNullOrEmpty());

        //    var activities = await ideGraph.ListActivities(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

        //    Assert.IsNotNull(activities);
        //    Assert.AreEqual(1, activities.Count);
        //    Assert.IsTrue(activities.Any(a => a.ID == activity.ID));

        //    var status = await ideGraph.DeleteActivity(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup);

        //    Assert.IsNotNull(status);
        //    Assert.IsTrue(status);

        //    activities = await ideGraph.ListActivities(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

        //    Assert.IsNotNull(activities);
        //    Assert.AreEqual(0, activities.Count);
        //    Assert.IsFalse(activities.Any(a => a.ID == activity.ID));
        //}
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
