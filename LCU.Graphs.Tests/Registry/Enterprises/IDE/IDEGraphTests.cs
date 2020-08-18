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

namespace LCU.Graphs.Tests.Registry.Enterprises.IDE
{
    [TestClass]
    public class IDEGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string testActivity = "qatest";

        protected readonly IDEGraph ideGraph;

        protected readonly ProvisioningGraph prvGraph;
        #endregion

        #region Constructors
        public IDEGraphTests()
            : base()
        {
            ideGraph = new IDEGraph(graphConfig, createLogger<IDEGraph>());

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

            await ideGraph.EnsureIDESettings(mainEnt.EnterpriseLookup, new IDEContainer()
            {
                Container = mainEnt.EnterpriseLookup
            });
        }
        #endregion

        #region API Methods
        [TestMethod]
        public async Task SaveListRemoveActivity()
        {
            var expected = new IDEActivity()
            {
                Icon = "dashboard",
                Lookup = testActivity,
                Sections = new[] { "first", "second" },
                Title = "Dashboard"
            };

            var activity = await ideGraph.SaveActivity(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, expected);

            Assert.IsNotNull(activity);
            Assert.AreNotEqual(Guid.Empty, activity.ID);
            Assert.AreEqual(expected.EnterpriseLookup, activity.EnterpriseLookup);
            Assert.AreEqual(expected.Icon, activity.Icon);
            Assert.AreEqual(expected.Lookup, activity.Lookup);
            Assert.AreEqual(expected.Registry, activity.Registry);
            Assert.AreEqual(expected.Title, activity.Title);
            Assert.IsFalse(activity.Sections.IsNullOrEmpty());

            var activities = await ideGraph.ListActivities(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count);
            Assert.IsTrue(activities.Any(a => a.ID == activity.ID));

            var status = await ideGraph.DeleteActivity(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            activities = await ideGraph.ListActivities(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

            Assert.IsNotNull(activities);
            Assert.AreEqual(0, activities.Count);
            Assert.IsFalse(activities.Any(a => a.ID == activity.ID));
        }

        [TestMethod]
        public async Task SaveListRemoveLCU()
        {
            var expected = new LCUConfig()
            {
                CapabilityFiles = new[] { "afile.file" },
                Lookup = "testlcu",
                NPMPackage = "npm-package",
                PackageVersion = "version",
                Modules = new ModulePackSetup()
                {
                    Displays = new[]
                    {
                        new ModuleDisplay()
                        {
                            Actions = new[]
                            {
                                new ModuleAction()
                                {
                                    Action = "action",
                                    Disabled = false,
                                    Icon = new LCUIcon()
                                    {
                                        Icon = "icon"
                                    },
                                    Order = 1,
                                    Text = "text"
                                }
                            },
                            Category = "category",
                            Element = "element",
                            Height = 100,
                            Icon = new LCUIcon()
                            {
                                Icon = "icon"
                            },
                            Left = 100,
                            ModuleType = "module-type",
                            Shape = ModuleShapeTypes.Circle,
                            Toolkit = "toolkit",
                            Top = 100,
                            Width = 100
                        }
                    },
                    Options = new[]
                    {
                        new ModuleOption()
                        {
                            Active = true,
                            ControlType = ModuleControlType.Direct,
                            Description = "description",
                            IncomingConnectionLimit = 1,
                            IncomingConnectionTypes = new[] { "incoming" },
                            ModuleType = "module-type",
                            Name = "name",
                            OutgoingConnectionLimit = 1,
                            OutgoingConnectionTypes = new[] { "outgoing" },
                            Settings = new MetadataModel()
                            {
                                Metadata = new Dictionary<string, JToken>()
                                {
                                    { "hello", "world" }
                                }
                            },
                            Visible = true
                        }
                    },
                    Pack = new ModulePack()
                    {
                        Description = "description",
                        Lookup = "lookup",
                        Name = "name",
                        Toolkit = "toolkit"
                    }
                },
                Solutions = new[]
                {
                    new IdeSettingsConfigSolution()
                    {
                        Element = "eleemnt",
                        Name = "name"
                    }
                },
                StateConfig = new MetadataModel()
                {
                    Metadata = new Dictionary<string, JToken>()
                    {
                        { "hello", "world" }
                    }
                }
            };

            var lcu = await ideGraph.SaveLCU(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, expected);

            Assert.IsNotNull(lcu);
            Assert.AreNotEqual(Guid.Empty, lcu.ID);
            Assert.IsFalse(lcu.CapabilityFiles.IsNullOrEmpty());
            Assert.AreEqual(expected.EnterpriseLookup, lcu.EnterpriseLookup);
            Assert.AreEqual(expected.NPMPackage, lcu.NPMPackage);
            Assert.AreEqual(expected.PackageVersion, lcu.PackageVersion);
            Assert.AreEqual(expected.Lookup, lcu.Lookup);
            Assert.AreEqual(expected.Registry, lcu.Registry);
            Assert.IsNotNull(lcu.Modules);
            Assert.IsFalse(lcu.Modules.Displays.IsNullOrEmpty());
            Assert.IsFalse(lcu.Modules.Options.IsNullOrEmpty());
            Assert.IsNotNull(lcu.Modules.Pack);

            var lcus = await ideGraph.ListLCUs(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

            Assert.IsNotNull(lcus);
            Assert.AreEqual(1, lcus.Count);
            Assert.IsTrue(lcus.Any(a => a.ID == lcu.ID));

            var status = await ideGraph.DeleteLCU(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, lcu.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            lcus = await ideGraph.ListLCUs(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup);

            Assert.IsNotNull(lcus);
            Assert.AreEqual(0, lcus.Count);
            Assert.IsFalse(lcus.Any(a => a.ID == lcu.ID));
        }
        #endregion

        #region Helpers
        #endregion
    }
}
