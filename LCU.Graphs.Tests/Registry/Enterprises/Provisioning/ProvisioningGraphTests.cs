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
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Provisioning
{
    [TestClass]
    public class ProvisioningGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string testActivity = "qatest";

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
        protected virtual LCUEnvironment createEnvironment()
        {
            return new LCUEnvironment()
            {

            };
        }

        protected virtual LCUEnvironmentSettings createEnvironmentSettings()
        {
            return new LCUEnvironmentSettings()
            {

            };
        }

        protected virtual SourceControl createSourceControl()
        {
            return new SourceControl()
            {

            };
        }
        #endregion
    }
}
