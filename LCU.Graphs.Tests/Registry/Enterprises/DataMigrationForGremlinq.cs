using ExRam.Gremlinq.Core;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Testing.Graphs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises
{
    [TestClass]
    public class DataMigrationForGremlinq : GenericGraphTests
    {
        #region Fields
        protected readonly ApplicationGraph appGraph;
        #endregion

        #region Constructors
        public DataMigrationForGremlinq()
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
        }
        #endregion

        #region API Methods
        //[TestMethod]
        public async Task MigrateEntLookupValues()
        {
            //	Update all records to set EnterpriseLookup from PrimaryAPIKey or EnterpriseAPIKey values

            var allEntities = await entGraph.g.V<LCUVertex>().ToListAsync();

            var unmapped = allEntities.Where(e => e.EnterpriseLookup.IsNullOrEmpty()).ToList();

            var failed = new List<LCUVertex>();

            await unmapped.Each(async entity =>
            {
                if (!entity.PrimaryAPIKey.IsNullOrEmpty())
                    entity.EnterpriseLookup = entity.PrimaryAPIKey;
                else if (!entity.EnterpriseAPIKey.IsNullOrEmpty())
                    entity.EnterpriseLookup = entity.EnterpriseAPIKey;

                entity = await entGraph.g.V<LCUVertex>(entity.ID)
                    .Update(entity)
                    .FirstOrDefaultAsync();

                if (entity == null || entity.EnterpriseLookup.IsNullOrEmpty())
                    failed.Add(entity);
            });
        }

        //[TestMethod]
        public async Task MigrateDAFApplications()
        {
            //	Update all DAF Applications so that extra details are on the .Details property

            var allDafApps = await entGraph.g.V<DAFApplication>().ToListAsync();

            await allDafApps.Each(async dafApp =>
            {
                if (!dafApp.BaseHref.IsNullOrEmpty())
                {
                    dafApp.Details = new DAFViewApplicationDetails()
                    {
                        BaseHref = dafApp.BaseHref,
                        NPMPackage = dafApp.NPMPackage,
                        PackageVersion = dafApp.PackageVersion,
                        StateConfig = dafApp.StateConfig
                    };
                }
                else if (!dafApp.APIRoot.IsNullOrEmpty())
                {
                    dafApp.Details = new DAFAPIApplicationDetails()
                    {
                        APIRoot = dafApp.APIRoot,
                        InboundPath = dafApp.InboundPath,
                        Methods = dafApp.Methods,
                        Security = dafApp.Security
                    };
                }
                else if (!dafApp.Redirect.IsNullOrEmpty())
                {
                    dafApp.Details = new DAFRedirectApplicationDetails()
                    {
                        Redirect = dafApp.Redirect
                    };
                }
                else if (!dafApp.DAFApplicationID.IsNullOrEmpty())
                {
                    dafApp.Details = new DAFAppPointerApplicationDetails()
                    {
                        DAFApplicationID = dafApp.DAFApplicationID,
                        DAFApplicationRoot = dafApp.DAFApplicationRoot
                    };
                }

                await entGraph.g.V<DAFApplication>(dafApp.ID)
                    .Update(dafApp)
                    .FirstOrDefaultAsync();
            });
        }

        //[TestMethod]
        public async Task MigrateLabels()
        {
            //	Update all DAF Applications so that extra details are on the .Details property

            //	Copy objects with Labels to new labels:
            //		- Activity => IDEActivity
            //		- 
        }
        #endregion
    }
}
