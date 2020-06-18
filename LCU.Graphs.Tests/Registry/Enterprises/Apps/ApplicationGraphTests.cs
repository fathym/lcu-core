using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Testing.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public virtual async Task Cleanup()
        {
            await cleanupEnterprises();
        }

        [TestInitialize]
        public virtual async Task Initialize()
        {
            await setupMainEnt(entGraph, appGraph);
        }
        #endregion

        #region API Methods
        [TestMethod]
        public async Task SaveListRemoveApplication()
        {
            var expected = createTestApplication();

            var app = await appGraph.Save(expected);

            Assert.IsNotNull(app);
            Assert.AreNotEqual(Guid.Empty, app.ID);
            Assert.AreEqual(expected.Container, app.Container);
            Assert.AreEqual(expected.Description, app.Description);
            Assert.AreEqual(expected.EnterpriseLookup, app.EnterpriseLookup);
            Assert.IsTrue(expected.IsPrivate);
            Assert.IsTrue(expected.IsReadOnly);
            Assert.AreEqual(expected.Name, app.Name);
            Assert.AreEqual(expected.PathRegex, app.PathRegex);
            Assert.AreEqual(expected.Priority, app.Priority);
            Assert.AreEqual(expected.QueryRegex, app.QueryRegex);
            Assert.AreEqual(expected.Registry, app.Registry);
            Assert.AreEqual(expected.UserAgentRegex, app.UserAgentRegex);
            Assert.IsTrue(app.AccessRights.Contains(accessRight));
            Assert.AreEqual(1, app.AccessRights.Length);
            Assert.IsTrue(app.Hosts.Contains(mainHost));
            Assert.AreEqual(1, app.Hosts.Length);
            Assert.IsTrue(app.Licenses.Contains(license));
            Assert.AreEqual(1, app.Licenses.Length);

            var apps = await appGraph.ListApplications(mainEnt.EnterpriseLookup);

            Assert.IsNotNull(apps);
            Assert.AreEqual(1, apps.Count);
            Assert.IsTrue(apps.Any(a => a.ID == app.ID));

            var status = await appGraph.RemoveApplication(app.ID);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            apps = await appGraph.ListApplications(mainEnt.EnterpriseLookup);

            Assert.IsNotNull(apps);
            Assert.AreEqual(0, apps.Count);
            Assert.IsFalse(apps.Any(a => a.ID == app.ID));
        }

        [TestMethod]
        public async Task SaveListRemoveDAFApplication()
        {
            var app = await appGraph.Save(createTestApplication());

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

            var viewApp = dafApp.JSONConvert<DAFViewConfiguration>();

            Assert.IsNotNull(viewApp);
            Assert.AreEqual(expected, dafApp.Registry);
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
                IsPrivate = true,
                IsReadOnly = true,
                PathRegex = "*",
                Priority = 100,
                QueryRegex = "*",
                Registry = mainEnt.EnterpriseLookup,
                UserAgentRegex = "*",
                EnterpriseLookup = mainEnt.EnterpriseLookup,
                AccessRights = new string[] { accessRight },
                Hosts = new string[] { mainHost },
                Licenses = new string[] { license }
            };
        }

        protected virtual DAFApplicationConfiguration createTestDAFApplication(Guid appId)
        {
            var rand = Guid.NewGuid();

            return new DAFViewConfiguration()
            {
                ApplicationID = appId,
                BaseHref = "/something/",
                EnterpriseLookup = mainEnt.EnterpriseLookup,
                Lookup = "something",
                NPMPackage = "@habistack/lcu-fathym-forecast-lcu",
                PackageVersion = "latest",
                Priority = 100,
                Registry = appId.ToString(),
                StateConfig = new Fathym.MetadataModel()
                {
                    Metadata = new Dictionary<string, JToken>()
                    {
                        { "hello", "world" }
                    }
                }
            };
        }
        #endregion
    }
}
