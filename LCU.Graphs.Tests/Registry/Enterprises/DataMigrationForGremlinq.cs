using ExRam.Gremlinq.Core;
using Fathym;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Graphs.Registry.Enterprises.Provisioning;
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
        ////[TestMethod]
        //public async Task MigrateActivity()
        //{
        //    //	Update all Activities so that .Section becomes .Sections

        //    var activities = await entGraph.g.V<Activity>().ToListAsync();

        //    await activities.Each(async act =>
        //    {
        //        if (!act.Section.IsNullOrEmpty())
        //            act.Sections = act.Section;

        //        act.Sections = act.Sections.Distinct().ToArray();

        //        await entGraph.g.V<Activity>(act.ID)
        //            .Update(act)
        //            .FirstOrDefaultAsync();
        //    });
        //}

        ////[TestMethod]
        //public async Task MigrateEntLookupValues()
        //{
        //    //	Update all records to set EnterpriseLookup from PrimaryAPIKey or EnterpriseAPIKey values

        //    var allEntities = await entGraph.g.V<LCUVertex>().ToListAsync();

        //    var unmapped = allEntities.Where(e => e.EnterpriseLookup.IsNullOrEmpty()).ToList();

        //    var failed = new List<LCUVertex>();

        //    await unmapped.Each(async entity =>
        //    {
        //        if (!entity.PrimaryAPIKey.IsNullOrEmpty())
        //            entity.EnterpriseLookup = entity.PrimaryAPIKey;
        //        else if (!entity.EnterpriseAPIKey.IsNullOrEmpty())
        //            entity.EnterpriseLookup = entity.EnterpriseAPIKey;

        //        entity = await entGraph.g.V<LCUVertex>(entity.ID)
        //            .Update(entity)
        //            .FirstOrDefaultAsync();

        //        if (entity == null || entity.EnterpriseLookup.IsNullOrEmpty())
        //            failed.Add(entity);
        //    });
        //}

        [TestMethod]
        public async Task MigrateApplicationLookups()
        {
            //	Update all DAF Applications so that extra details are on the .Details property

            var allApps = await entGraph.g.V<Application>().ToListAsync();

            await allApps.Each(async app =>
            {
                var config = app.Config?.JSONConvert<ApplicationLookupConfiguration>();

                if (config == null || config.PathRegex.IsNullOrEmpty())
                {
                    app.Config = new ApplicationLookupConfiguration()
                    {
                        AccessRights = app.AccessRights.ToList(),
                        AccessRightsAllAny = AllAnyTypes.Any,
                        IsPrivate = app.IsPrivate,
                        IsReadOnly = app.IsReadOnly,
                        IsTriggerSignIn = app.IsPrivate,
                        Licenses = app.Licenses.ToList(),
                        LicensesAllAny = AllAnyTypes.All,
                        PathRegex = app.PathRegex,
                        QueryRegex = app.QueryRegex,
                        UserAgentRegex = app.UserAgentRegex
                    }.JSONConvert<MetadataModel>();

                    await entGraph.g.V<Application>(app.ID)
                        .Update(app)
                        .FirstOrDefaultAsync();
                }
            });
        }

        //[TestMethod]
        //public async Task MigrateDAFApplications()
        //{
        //    //	Update all DAF Applications so that extra details are on the .Details property

        //    var allDafApps = await entGraph.g.V<DAFApplication>().ToListAsync();

        //    await allDafApps.Each(async dafApp =>
        //    {
        //        var details = dafApp.Details.JSONConvert<DAFViewApplicationDetails>();

        //        if (!details.NPMPackage.IsNullOrEmpty())
        //        {
        //            dafApp.Details.Metadata["Package"] = new DAFApplicationNPMPackage()
        //            {
        //                Name = details.NPMPackage,
        //                Version = details.PackageVersion
        //            }.JSONConvert<JToken>();

        //            dafApp.Details.Metadata["PackageType"] = DAFApplicationPackageTypes.NPM.ToString();

        //            await entGraph.g.V<DAFApplication>(dafApp.ID)
        //                .Update(dafApp)
        //                .FirstOrDefaultAsync();
        //        }
        //    });
        //}

        //[TestMethod]
        //public async Task MigrateLCUConfigs()
        //{
        //    //	Update all DAF Applications so that extra details are on the .Details property

        //    var lcuConfigs = await entGraph.g.V<LCUConfig>().ToListAsync();

        //    await lcuConfigs.Each(async lcuConfig =>
        //    {
        //        if (!lcuConfig.NPMPackage.IsNullOrEmpty())
        //        {
        //            lcuConfig.Package = new DAFApplicationNPMPackage()
        //            {
        //                Name = lcuConfig.NPMPackage,
        //                Version = lcuConfig.PackageVersion
        //            }.JSONConvert<MetadataModel>();

        //            lcuConfig.PackageType = DAFApplicationPackageTypes.NPM;

        //            await entGraph.g.V<LCUConfig>(lcuConfig.ID)
        //                .Update(lcuConfig)
        //                .FirstOrDefaultAsync();
        //        }
        //    });
        //}

        ////[TestMethod]
        //public async Task MigrateEnvironmentSettings()
        //{
        //    //	Update all DAF Applications so that extra details are on the .Details property

        //    var envSettings = await entGraph.g.V<EnvironmentSettings>().ToListAsync();

        //    await envSettings.Each(async envSetting =>
        //    {
        //        envSetting.Settings = new
        //        {
        //            AzureAppAuthKey = envSetting.AzureAppAuthKey,
        //            AzureAppID = envSetting.AzureAppID,
        //            AzureDevOpsProjectID = envSetting.AzureDevOpsProjectID,
        //            AzureFeedID = envSetting.AzureFeedID,
        //            AzureInfrastructureServiceEndpointID = envSetting.AzureInfrastructureServiceEndpointID,
        //            AzureLocation = envSetting.AzureLocation,
        //            AzureRegion = envSetting.AzureRegion,
        //            AzureSubID = envSetting.AzureSubID,
        //            AzureTenantID = envSetting.AzureTenantID,
        //            EnvironmentInfrastructureTemplate = envSetting.EnvironmentInfrastructureTemplate,
        //            EnvironmentLookup = envSetting.EnvironmentLookup,
        //            InfrastructureRepoName = envSetting.InfrastructureRepoName,
        //            OrganizationLookup = envSetting.OrganizationLookup
        //        }.JSONConvert<MetadataModel>();

        //        await entGraph.g.V<EnvironmentSettings>(envSetting.ID)
        //            .Update(envSetting)
        //            .FirstOrDefaultAsync();
        //    });
        //}

        ////[TestMethod]
        //public async Task MigrateLabels()
        //{
        //    //	Update all DAF Applications so that extra details are on the .Details property

        //    //	Copy objects with Labels to new labels:
        //    //		- ModuleOption??
        //    //		- ModuleDisplay??
        //    //      - SectionAction

        //    //  Maybe don't need this now
        //}

        ////[TestMethod]
        //public async Task MigrateViewAndLCUPackages()
        //{
        //    //	Update all DAF Applications so that extra details are on the .Details property

        //    //	Copy objects with Labels to new labels:
        //    //		- ModuleOption??
        //    //		- ModuleDisplay??
        //    //      - SectionAction

        //    //  Maybe don't need this now
        //}
        #endregion
    }
}
