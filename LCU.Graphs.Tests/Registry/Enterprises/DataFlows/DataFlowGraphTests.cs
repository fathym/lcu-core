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

namespace LCU.Graphs.Tests.Registry.Enterprises.DataFlows
{
    [TestClass]
    public class DataFlowGraphTests : GenericGraphTests
    {
        #region Fields
        protected readonly string dataFlowSuffix = "qatest";

        protected readonly DataFlowGraph dfGraph;

        protected readonly ProvisioningGraph prvGraph;
        #endregion

        #region Constructors
        public DataFlowGraphTests()
            : base()
        {
            dfGraph = new DataFlowGraph(graphConfig, createLogger<DataFlowGraph>());

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
        public async Task SaveListRemoveDataFlow()
        {
            var envLookup = mainEnv.Lookup;

            var expected = createTestDataFlow(envLookup);

            var dataFlow = await dfGraph.SaveDataFlow(mainEnt.EnterpriseLookup, envLookup, expected);

            Assert.IsNotNull(dataFlow);
            Assert.AreNotEqual(Guid.Empty, dataFlow.ID);
            Assert.AreEqual(expected.Description, dataFlow.Description);
            Assert.AreEqual(expected.EnterpriseLookup, dataFlow.EnterpriseLookup);
            Assert.AreEqual(expected.Name, dataFlow.Name);
            Assert.AreEqual(expected.Lookup, dataFlow.Lookup);
            Assert.AreEqual(expected.Registry, dataFlow.Registry);
            Assert.IsFalse(dataFlow.ModulePacks.IsNullOrEmpty());
            Assert.IsNotNull(dataFlow.Output);
            Assert.IsFalse(dataFlow.Output.Modules.IsNullOrEmpty());
            Assert.IsFalse(dataFlow.Output.Streams.IsNullOrEmpty());

            var dataFlows = await dfGraph.ListDataFlows(mainEnt.EnterpriseLookup, envLookup);

            Assert.IsNotNull(dataFlows);
            Assert.AreEqual(1, dataFlows.Count);
            Assert.IsTrue(dataFlows.Any(a => a.ID == dataFlow.ID));

            var status = await dfGraph.DeleteDataFlow(mainEnt.EnterpriseLookup, envLookup, dataFlow.Lookup);

            Assert.IsNotNull(status);
            Assert.IsTrue(status);

            dataFlows = await dfGraph.ListDataFlows(mainEnt.EnterpriseLookup, envLookup);

            Assert.IsNotNull(dataFlows);
            Assert.AreEqual(0, dataFlows.Count);
            Assert.IsFalse(dataFlows.Any(a => a.ID == dataFlow.ID));
        }
        #endregion

        #region Helpers
        protected override string buildEnvironmentLookup()
        {
            return $"{orgLookup}-{dataFlowSuffix}";
        }

        protected virtual DataFlow createTestDataFlow(string envLookup)
        {
            var safeEnvLookup = envLookup.Replace("-", "");

            return new DataFlow()
            {
                Name = "IoT Getting Started Journey",
                Description = "The IoT Getting Started flow will help you leverage the power of an azure best practice cloud infrastructure or your IoT project.",
                Lookup = "iot",
                ModulePacks = new string[]
                {
                    "this-is-a-pack"
                },
                Output = new DataFlowOutput()
                {
                    Modules = new Module[]
                    {
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Emulators",
                                Element = "lcu-data-flow-iot-provisioning-pack-data-emulator-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "input" },
                                ModuleType = "data-emulator",
                                Shape = ModuleShapeTypes.Rectangle,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Width = 200
                            },
                            ID = new Guid("111fb8c3-0e30-46de-a61c-b78fe1b9d5dd"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup}-fn - data-emulator",
                                    Group = $"{envLookup}-fn",
                                    Lookup = $"data-emulator|{envLookup}-fn|data-emulator",
                                    Name = "data-emulator"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Data Emulator"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Data Sources",
                                Element = "lcu-data-flow-iot-provisioning-pack-data-stream-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "settings_input_antenna" },
                                Left = 350,
                                ModuleType = "data-stream",
                                Shape = ModuleShapeTypes.Rectangle,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Width = 200
                            },
                            ID = new Guid("39240064-dca2-4a18-9377-777d0e4d29db"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup}-dslcu",
                                    Group = $"{envLookup}-ds",
                                    Lookup = $"data-stream|{envLookup}-ds|{envLookup}-dslcu",
                                    Name = $"{envLookup}-dslcu"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Data Stream"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Translation",
                                Element = "lcu-data-flow-iot-provisioning-pack-data-map-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "call_merge" },
                                Left = 700,
                                ModuleType = "data-map",
                                Shape = ModuleShapeTypes.Custom,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Width = 200
                            },
                            ID = new Guid("4709ff32-3bd8-4535-950d-02518fa61d7f"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup}-lcu-asa-data-map",
                                    Group = "",
                                    Lookup = $"data-map|{envLookup}-lcu-asa-data-map",
                                    Name = $"{envLookup}-lcu-asa-data-map"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Data Map"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Storage",
                                Element = "lcu-data-flow-iot-provisioning-pack-cold-storage-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "assessment" },
                                Left = 1050,
                                ModuleType = "cold-storage",
                                Shape = ModuleShapeTypes.Ellipse,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Top = -350,
                                Width = 200
                            },
                            ID = new Guid("8eb91baf-d4a4-4b9b-b941-3c05bc5cbbd0"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{safeEnvLookup}",
                                    Group = "",
                                    Lookup = $"cold-storage|{safeEnvLookup}",
                                    Name = $"{safeEnvLookup}"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Cold Storage"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Storage",
                                Element = "lcu-data-flow-iot-provisioning-pack-warm-storage-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "assessment" },
                                Left = 1050,
                                ModuleType = "warm-storage",
                                Shape = ModuleShapeTypes.Ellipse,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Width = 200
                            },
                            ID = new Guid("b123dda9-788d-47eb-8698-7d9c80817492"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup} - {envLookup}:telemetry",
                                    Group = $"{envLookup}",
                                    Lookup = $"warm-storage|{envLookup}|{envLookup}:telemetry",
                                    Name = $"{envLookup}:telemetry"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Warm Storage"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Storage",
                                Element = "lcu-data-flow-iot-provisioning-pack-hot-storage-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "dashboard" },
                                Left = 1050,
                                ModuleType = "hot-storage",
                                Shape = ModuleShapeTypes.Ellipse,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Top = 350,
                                Width = 200
                            },
                            ID = new Guid("42877965-0bed-4edf-9b99-e308a856c839"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup} - lcu",
                                    Group = $"{envLookup}",
                                    Lookup = $"hot-storage|{envLookup}|lcu",
                                    Name = "lcu"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Hot Storage"
                        },
                        new Module()
                        {
                            Display = new ModuleDisplay()
                            {
                                Category = "Query",
                                Element = "lcu-data-flow-iot-provisioning-pack-warm-query-element",
                                Height = 200,
                                Icon = new LCUIcon() { Icon = "search" },
                                Left = 1400,
                                ModuleType = "warm-query",
                                Shape = ModuleShapeTypes.Ellipse,
                                Toolkit = "https://limited.fathym-it.com/_lcu/lcu-data-flow-iot-provisioning-pack-lcu/wc/lcu-data-flow-iot-provisioning-pack.lcu.js",
                                Width = 200
                            },
                            ID = new Guid("5708f3b6-ecc0-4aed-9bcd-f389d8720c73"),
                            Settings = new
                            {
                                Infrastructure = new
                                {
                                    DisplayName = $"{envLookup}-fn - warm-query",
                                    Group = $"{envLookup}-fn",
                                    Lookup = $"warm-query|{envLookup}-fn|warm-query",
                                    Name = "warm-query"
                                }
                            }.JSONConvert<MetadataModel>(),
                            Text = "Warm Query"
                        }
                    },
                    Streams = new ModuleStream[]
                    {
                        new ModuleStream()
                        {
                            ID = new Guid("6577aa04-b87b-40ac-92f8-e70311f0ce88"),
                            InputModuleID = new Guid("111fb8c3-0e30-46de-a61c-b78fe1b9d5dd"),
                            OutputModuleID = new Guid("39240064-dca2-4a18-9377-777d0e4d29db")
                        },
                        new ModuleStream()
                        {
                            ID = new Guid("7440a431-3132-4aa3-8a59-e0a5aa797650"),
                            InputModuleID = new Guid("39240064-dca2-4a18-9377-777d0e4d29db"),
                            OutputModuleID = new Guid("4709ff32-3bd8-4535-950d-02518fa61d7f")
                        },
                        new ModuleStream()
                        {
                            ID = new Guid("99328b74-b17b-418a-ba52-539ce474439f"),
                            InputModuleID = new Guid("4709ff32-3bd8-4535-950d-02518fa61d7f"),
                            OutputModuleID = new Guid("8eb91baf-d4a4-4b9b-b941-3c05bc5cbbd0")
                        },
                        new ModuleStream()
                        {
                            ID = new Guid("abf59a69-bae7-4979-8eb0-f9c5c5326adc"),
                            InputModuleID = new Guid("4709ff32-3bd8-4535-950d-02518fa61d7f"),
                            OutputModuleID = new Guid("b123dda9-788d-47eb-8698-7d9c80817492")
                        },
                        new ModuleStream()
                        {
                            ID = new Guid("9ada053f-af9d-43ab-8d7e-3343ef48fcf9"),
                            InputModuleID = new Guid("4709ff32-3bd8-4535-950d-02518fa61d7f"),
                            OutputModuleID = new Guid("42877965-0bed-4edf-9b99-e308a856c839")
                        },
                        new ModuleStream()
                        {
                            ID = new Guid("d4ac16ce-76f5-49a8-abe6-bd4a3cce5a1f"),
                            InputModuleID = new Guid("b123dda9-788d-47eb-8698-7d9c80817492"),
                            OutputModuleID = new Guid("5708f3b6-ecc0-4aed-9bcd-f389d8720c73")
                        },
                    }
                }
            };
        }
        #endregion
    }
}
