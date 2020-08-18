using LCU.Graphs.Registry.Enterprises.IDE;
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

        protected readonly IDEGraph ideGraph;
        #endregion

        #region Constructors
        public IDEGraphTests()
            : base()
        {
            ideGraph = new IDEGraph(graphConfig, createLogger<IDEGraph>());
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
            //await setupMainEnt(entGraph, IDEGraph);
        }
        #endregion

        #region API Methods

        #endregion

        #region Helpers

        #endregion
    }
}
