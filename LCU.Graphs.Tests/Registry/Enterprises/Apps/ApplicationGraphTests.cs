using Fathym;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Testing.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Apps
{
    [TestClass]
    public class ApplicationGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string accessRight = "LCU.Test.Everything";

        protected readonly ApplicationGraph appGraph;

        protected readonly string license = "lcu";
        #endregion

        #region Constructors
        public ApplicationGraphTests()
            : base()
        {
            appGraph = new ApplicationGraph(graphConfig, createLogger<ApplicationGraph>());
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
            await setupMainEnt(entGraph, appGraph);
        }
        #endregion

        #region API Methods
        [TestMethod]
        public async Task SaveListRemoveApplication()
        {
            var expected = createTestApplication();

            var app = await appGraph.Save(mainEnt.EnterpriseLookup, expected);

            var appConfig = app.Config.JSONConvert<ApplicationLookupConfiguration>();

            var expectedConfig = expected.Config.JSONConvert<ApplicationLookupConfiguration>();

            Assert.IsNotNull(app);
            Assert.AreNotEqual(Guid.Empty, app.ID);
            Assert.AreEqual(expected.Container, app.Container);
            Assert.AreEqual(expected.Description, app.Description);
            Assert.AreEqual(expected.EnterpriseLookup, app.EnterpriseLookup);
            Assert.IsTrue(expectedConfig.IsPrivate);
            Assert.IsTrue(expectedConfig.IsReadOnly);
            Assert.AreEqual(expected.Name, app.Name);
            Assert.AreEqual(expectedConfig.PathRegex, appConfig.PathRegex);
            Assert.AreEqual(expected.Priority, app.Priority);
            Assert.AreEqual(expectedConfig.QueryRegex, appConfig.QueryRegex);
            Assert.AreEqual(expected.Registry, app.Registry);
            Assert.AreEqual(expectedConfig.UserAgentRegex, appConfig.UserAgentRegex);
            Assert.IsTrue(appConfig.AccessRights.Contains(accessRight));
            Assert.AreEqual(1, appConfig.AccessRights.Count);
            Assert.AreEqual(expectedConfig.AccessRightsAllAny, appConfig.AccessRightsAllAny);
            Assert.IsTrue(app.Hosts.Contains(mainHost));
            Assert.AreEqual(1, app.Hosts.Length);
            Assert.IsTrue(appConfig.Licenses.Contains(license));
            Assert.AreEqual(1, appConfig.Licenses.Count);

            expected = createTestApplication();

            expected.Priority += expected.Priority;

            app = await appGraph.Save(mainEnt.EnterpriseLookup, expected);

            var apps = await appGraph.ListApplications(mainEnt.EnterpriseLookup);

            Assert.IsNotNull(apps);
            Assert.AreEqual(2, apps.Count);
            Assert.IsTrue(apps.Any(a => a.ID == app.ID));
            Assert.IsTrue(apps.First().Priority > apps.Last().Priority);

            var status = await appGraph.RemoveApplication(app.ID);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            apps = await appGraph.ListApplications(mainEnt.EnterpriseLookup);

            Assert.IsNotNull(apps);
            Assert.AreEqual(1, apps.Count);
            Assert.IsFalse(apps.Any(a => a.ID == app.ID));
        }

        [TestMethod]
        public async Task SaveListRemoveDAFApplication()
        {
            var app = await appGraph.Save(mainEnt.EnterpriseLookup, createTestApplication());

            Assert.IsNotNull(app);
            Assert.AreNotEqual(Guid.Empty, app.ID);

            var expected = createTestDAFApplication(app.ID);

            var dafApp = await appGraph.SaveDAFApplication(mainEnt.EnterpriseLookup, expected);

            Assert.IsNotNull(dafApp);
            Assert.AreNotEqual(Guid.Empty, dafApp.ID);
            Assert.AreEqual(expected.ApplicationID, dafApp.ApplicationID);
            Assert.AreEqual(expected.EnterpriseLookup, dafApp.EnterpriseLookup);
            Assert.AreEqual(expected.Lookup, dafApp.Lookup);
            Assert.AreEqual(expected.Priority, dafApp.Priority);
            Assert.AreEqual(expected.Registry, dafApp.Registry);

            var viewDetails = dafApp.Details?.JSONConvert<DAFViewApplicationDetails>();

            Assert.IsNotNull(viewDetails);
            Assert.AreEqual("world", viewDetails.StateConfig.Metadata["hello"].ToString());

            expected = createTestDAFApplication(app.ID);

            expected.Priority += expected.Priority;

            dafApp = await appGraph.SaveDAFApplication(mainEnt.EnterpriseLookup, expected);

            var dafApps = await appGraph.ListDAFApplications(mainEnt.EnterpriseLookup, app.ID);

            Assert.IsNotNull(dafApps);
            Assert.AreEqual(2, dafApps.Count);
            Assert.IsTrue(dafApps.Any(a => a.ID == dafApp.ID));
            Assert.IsTrue(dafApps.First().Priority > dafApps.Last().Priority);

            var status = await appGraph.RemoveDAFApplication(dafApp.ID);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            dafApps = await appGraph.ListDAFApplications(mainEnt.EnterpriseLookup, app.ID);

            Assert.IsNotNull(dafApps);
            Assert.AreEqual(1, dafApps.Count);
            Assert.IsFalse(dafApps.Any(a => a.ID == dafApp.ID));
        }
        #endregion

        #region Helpers
        protected virtual Application createTestApplication()
        {
            var rand = Guid.NewGuid();

            return new Application()
            {
                Name = $"{GetType().FullName}-Test-{rand}",
                Description = "A description",
                Container = "test-data-app",
                Config = new ApplicationLookupConfiguration()
                {
                    IsPrivate = true,
                    IsReadOnly = true,
                    PathRegex = "*",
                    Licenses = new List<string>() { license },
                    QueryRegex = "*",
                    UserAgentRegex = "*",
                    AccessRights = new List<string>() { accessRight },
                    AccessRightsAllAny = AllAnyTypes.Any
                },
                Priority = 100,
                Hosts = new string[] { mainHost }
            };
        }

        protected virtual DAFApplication createTestDAFApplication(Guid appId)
        {
            var rand = Guid.NewGuid();

            return new DAFApplication()
            {
                ApplicationID = appId.ToString(),
                Lookup = "something",
                Priority = 100,
                Details = new DAFViewApplicationDetails()
                {
                    BaseHref = "/something/",
                    Package = new DAFApplicationNPMPackage()
                    {
                        Name = "@habistack/lcu-fathym-forecast-lcu",
                        Version = "latest"
                    }.JSONConvert<MetadataModel>(),
                    PackageType = DAFApplicationPackageTypes.NPM,
                    RegScripts = "function() { alert('Hey'); }",
                    StateConfig = new MetadataModel()
                    {
                        Metadata = new Dictionary<string, JToken>()
                        {
                            { "hello", "world" }
                        }
                    }
                }.JSONConvert<MetadataModel>()
            };
        }
        #endregion
    }
}
