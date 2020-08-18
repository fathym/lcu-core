using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Testing.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Identity
{
    [TestClass]
    public class IdentityGraphTests : GenericGraphTests
    {
        #region Fields

        protected readonly IdentityGraph identityGraph;
        #endregion

        #region Constructors
        public IdentityGraphTests()
            : base()
        {
            identityGraph = new IdentityGraph(graphConfig, createLogger<IdentityGraph>());
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
            //await setupMainEnt(entGraph, identityGraph);
        }
        #endregion

        #region API Methods

        #endregion

        #region Helpers

        #endregion
    }
}
