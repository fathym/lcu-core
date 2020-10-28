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
        public override async Task Cleanup()
        {
            await base.Cleanup();
        }

        [TestInitialize]
        public override async Task Initialize()
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
        public async Task ListParentLCUs()
        {
            var lcus = await ideGraph.ListLCUs(parentEntLookup, "Default");

            Assert.IsNotNull(lcus);
            Assert.AreNotEqual(0, lcus.Count);
            Assert.IsNotNull(lcus.First().Modules);
        }

        [TestMethod]
        public async Task ListParentSections()
        {
            var sections = await ideGraph.ListSideBarSections(parentEntLookup, "Default", "core");

            Assert.IsNotNull(sections);
            Assert.AreEqual(1, sections.Count);

            var actions = await ideGraph.ListSectionActions(parentEntLookup, "Default", "core", sections.First());

            Assert.IsNotNull(actions);
            Assert.AreEqual(3, actions.Count);
        }

        [TestMethod]
        public async Task ParallelSideBarActions()
        {
            var tasks = new List<Task>()
            {
                simpleManageSectionActions(),
                simpleManageSectionActions(),
                simpleManageSectionActions(),
                simpleManageSectionActions()
            };

            await Task.WhenAll(tasks.ToArray());
        }

        [TestMethod]
        public async Task ManageSectionActions()
        {
            var expected = new Activity()
            {
                Icon = "dashboard",
                Lookup = testActivity,
                Sections = Array.Empty<string>(),
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
            Assert.IsTrue(activity.Sections.IsNullOrEmpty());

            var status = await ideGraph.AddSideBarSection(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, "NewSection");

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            status = await ideGraph.AddSideBarSection(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, "NewSection2");

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            var sections = await ideGraph.ListSideBarSections(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup);

            Assert.IsNotNull(sections);
            Assert.AreEqual(2, sections.Count);
            Assert.IsTrue(sections.Any(sec => sec == "NewSection2"));

            status = await ideGraph.DeleteSideBarSection(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, "NewSection2");

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            sections = await ideGraph.ListSideBarSections(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup);

            Assert.IsNotNull(sections);
            Assert.AreEqual(1, sections.Count);
            Assert.IsTrue(sections.Any(sec => sec == "NewSection"));

            var expectedSecAct = new SectionAction()
            {
                Action = "Action",
                Group = "Group",
                Section = sections.First(),
                Title = "Title",
            };

            var secAct = await ideGraph.SaveSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, expectedSecAct);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct.Section, sections.First());
            Assert.AreEqual(expectedSecAct.Title, secAct.Title);

            var expectedSecAct2 = new SectionAction()
            {
                Action = "Action2",
                Group = "Group2",
                Section = sections.First(),
                Title = "Title2",
            };

            secAct = await ideGraph.SaveSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, expectedSecAct2);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct2.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct2.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct2.Section, sections.First());
            Assert.AreEqual(expectedSecAct2.Title, secAct.Title);

            var secActs = await ideGraph.ListSectionActions(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First());

            Assert.IsNotNull(secActs);
            Assert.AreEqual(2, secActs.Count);

            status = await ideGraph.DeleteSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First(),
                secAct.Action, secAct.Group);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            secActs = await ideGraph.ListSectionActions(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First());

            Assert.IsNotNull(secActs);
            Assert.AreEqual(1, secActs.Count);

            secAct = await ideGraph.GetSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First(),
                expectedSecAct.Action, expectedSecAct.Group);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct.Section, sections.First());
            Assert.AreEqual(expectedSecAct.Title, secAct.Title);
        }

        [TestMethod]
        public async Task SaveListRemoveActivity()
        {
            var expected = new Activity()
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
                Package = new DAFApplicationNPMPackage()
                {
                    Name = "npm-package",
                    Version = "version",
                }.JSONConvert<MetadataModel>(),
                PackageType = DAFApplicationPackageTypes.NPM,
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
            Assert.AreEqual(expected.Package.Metadata["Name"], lcu.Package.Metadata["Name"]);
            Assert.AreEqual(expected.Package.Metadata["Version"], lcu.Package.Metadata["Version"]);
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
        protected async Task simpleManageSectionActions()
        {
            var expected = new Activity()
            {
                Icon = "dashboard",
                Lookup = testActivity,
                Sections = Array.Empty<string>(),
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
            Assert.IsTrue(activity.Sections.IsNullOrEmpty());

            var status = await ideGraph.AddSideBarSection(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, "NewSection");

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            status = await ideGraph.AddSideBarSection(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, "NewSection2");

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            var sections = await ideGraph.ListSideBarSections(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup);

            Assert.IsNotNull(sections);
            Assert.IsTrue(sections.Any(sec => sec == "NewSection2"));

            var expectedSecAct = new SectionAction()
            {
                Action = "Action",
                Group = "Group",
                Section = sections.First(),
                Title = "Title",
            };

            var secAct = await ideGraph.SaveSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, expectedSecAct);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct.Section, sections.First());
            Assert.AreEqual(expectedSecAct.Title, secAct.Title);

            var expectedSecAct2 = new SectionAction()
            {
                Action = "Action2",
                Group = "Group2",
                Section = sections.First(),
                Title = "Title2",
            };

            secAct = await ideGraph.SaveSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, expectedSecAct2);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct2.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct2.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct2.Section, sections.First());
            Assert.AreEqual(expectedSecAct2.Title, secAct.Title);

            var secActs = await ideGraph.ListSectionActions(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First());

            Assert.IsNotNull(secActs);

            secAct = await ideGraph.GetSectionAction(mainEnt.EnterpriseLookup, mainEnt.EnterpriseLookup, activity.Lookup, sections.First(),
                expectedSecAct.Action, expectedSecAct.Group);

            Assert.IsNotNull(secAct);
            Assert.AreEqual(expectedSecAct.Action, secAct.Action);
            Assert.AreEqual(expectedSecAct.Group, secAct.Group);
            Assert.AreEqual(expectedSecAct.Section, sections.First());
            Assert.AreEqual(expectedSecAct.Title, secAct.Title);
        }
        #endregion
    }
}
