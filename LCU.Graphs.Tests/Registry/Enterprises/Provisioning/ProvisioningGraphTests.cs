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

        protected readonly ProvisioningGraph provGraph;
        #endregion

        #region Constructors
        public ProvisioningGraphTests()
            : base()
        {
            provGraph = new ProvisioningGraph(graphConfig, createLogger<ProvisioningGraph>());
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
            //await setupMainEnt(entGraph, ProvisioningGraph);
        }
        #endregion

        #region API Methods

        #endregion

        #region Helpers

        #endregion
    }
}
