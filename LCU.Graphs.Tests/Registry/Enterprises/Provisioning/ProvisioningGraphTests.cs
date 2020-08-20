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
        public override async Task Cleanup()
        {
            await base.Cleanup();
        }

        [TestInitialize]
        public override async Task Initialize()
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
            var actual = await prvGraph.GetEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Settings);
            Assert.IsFalse(actual.Settings.Metadata["AzureTenantID"].ToString().IsNullOrEmpty());
            Assert.IsFalse(actual.Settings.Metadata["AzureSubID"].ToString().IsNullOrEmpty());

            actual.Settings.Metadata["NewValue"] = mainEnt.EnterpriseLookup;

            actual = await prvGraph.SaveEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup, new LCUEnvironmentSettings() { Settings = actual.Settings });

            actual = await prvGraph.GetEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Settings);
            Assert.IsFalse(actual.Settings.Metadata["AzureTenantID"].ToString().IsNullOrEmpty());
            Assert.IsFalse(actual.Settings.Metadata["AzureSubID"].ToString().IsNullOrEmpty());
            Assert.AreEqual(mainEnt.EnterpriseLookup, actual.Settings.Metadata["NewValue"].ToString());

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
