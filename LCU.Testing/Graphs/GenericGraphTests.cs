using Fathym;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Testing.Graphs
{
    public class GenericGraphTests : GenericTests
    {
        #region Fields
        protected readonly List<string> createdEntLookups;

        protected readonly EnterpriseGraph entGraph;

        protected readonly LCUGraphConfig graphConfig;

        protected readonly string hostRoot;

        protected Enterprise mainEnt;

        protected LCU.Graphs.Registry.Enterprises.Provisioning.Environment mainEnv;

        protected EnvironmentSettings mainEnvSettings;

        protected readonly string mainHost;

        protected readonly string orgLookup;

        protected readonly string parentEntLookup;

        protected readonly string username;
        #endregion

        #region Constructors
        public GenericGraphTests()
        {
            setupConfiguration();

            graphConfig = new LCUGraphConfig()
            {
                APIKey = config["LCU-GRAPH-API-KEY"],
                Database = config["LCU-GRAPH-DATABASE"],
                Host = config["LCU-GRAPH-HOST"],
                Graph = config["LCU-GRAPH"]
            };

            createdEntLookups = new List<string>();

            entGraph = new EnterpriseGraph(graphConfig, createLogger<EnterpriseGraph>());

            hostRoot = config["LCU-HOST-ROOT"];

            orgLookup = config["LCU-ORG-LOOKUP"] + Guid.NewGuid().ToString().Substring(8, 8);

            mainHost = $"{orgLookup}.{hostRoot}";

            parentEntLookup = config["LCU-PARENT-ENT-LOOKUP"];

            username = config["LCU-USERNAME"];
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
        { }
        #endregion

        #region Helpers
        protected virtual void addEntForCleanup(string entLookup)
        {
            createdEntLookups.Add(entLookup);
        }

        protected virtual string buildEnvironmentLookup()
        {
            return orgLookup;
        }

        protected virtual async Task cleanupEnterprises()
        {
            await createdEntLookups.Each(async entLookup =>
            {
                if (entLookup == "3ebd1c0d-22d0-489e-a46f-3260103c8cd7")
                    throw new Exception("This would blow up everything, so don't do it");

                await cleanupEnterprise(entLookup);
            });
        }

        protected virtual async Task cleanupEnterprise(string entLookup)
        {
            await entGraph.DeleteEnterprise(entLookup);
        } 

        protected virtual async Task<RelyingParty> loadDefaultRelyingParty()
        {
            //	TODO:  How to power this by open source repo config, and enable white labeling users to define their own fork
            var nideAccessRight = new AccessRight()
            {
                Lookup = "LCU.NapkinIDE.AllAccess",
                Name = "LCU Napkin IDE - All Access",
                Description = "Represents complete access to the enterprise Napkin IDE instance."
            };

            var nideStakeHolderAccessRight = new AccessRight()
            {
                Lookup = "LCU.NapkinIDE.StakeHolder",
                Name = "LCU Napkin IDE - Stake Holder",
                Description = "Represents stake holder access to the enterprise Napkin IDE instance."
            };

            var initialAccessConfig = new AccessConfiguration()
            {
                Type = "LCU",
                AcceptedProviderIDs = Array.Empty<Guid>(),
                AccessRights = new string[]
                {
                    nideAccessRight.Lookup,
                    nideStakeHolderAccessRight.Lookup
                }
            };

            return new RelyingParty()
            {
                AccessConfigurations = new AccessConfiguration[] { initialAccessConfig },
                AccessRights = new AccessRight[] { nideAccessRight, nideStakeHolderAccessRight },
                DefaultAccessConfigurationType = "LCU",
                Providers = Array.Empty<Provider>(),
                //	TODO:  Implement providers concept throughout... not needed for MVP, let's craft a story
            };
        }

        protected virtual async Task<AccessCard> loadDefaultAccessCard(RelyingParty relyingParty)
        {
            return new AccessCard()
            {
                AccessConfigurationType = relyingParty.DefaultAccessConfigurationType,
                ExcludeAccessRights = new List<string>().ToArray(),
                IncludeAccessRights = new List<string>().ToArray(),
                ID = Guid.NewGuid(),
                Registry = relyingParty.EnterpriseLookup,
                EnterpriseLookup = relyingParty.EnterpriseLookup,
            };
        }

        protected virtual async Task setupMainEnt(EnterpriseGraph entGraph, ApplicationGraph appGraph = null, ProvisioningGraph prvGraph = null, IdentityGraph idGraph = null)//, AzureDevOpsRepoManager devOpsRepoMgr)
        {
            Assert.AreNotEqual("www.fathym-int.com", mainHost, "This would blow up everything, so don't do it");
            Assert.AreNotEqual("www.fathym-it.com", mainHost, "This would blow up everything, so don't do it");

            mainEnt = await entGraph.LoadByHost(mainHost);

            if (mainEnt == null)
            {
                mainEnt = await entGraph.Create(mainHost, mainHost, mainHost);

                Assert.IsNotNull(mainEnt);

                addEntForCleanup(mainEnt.EnterpriseLookup);

                if (appGraph != null)
                {
                    var status = await appGraph.SeedDefault(parentEntLookup, mainEnt.EnterpriseLookup);

                    Assert.IsNotNull(status);
                    Assert.IsTrue(status);
                }

                if (prvGraph != null)
                {
                    mainEnv = await prvGraph.SaveEnvironment(mainEnt.EnterpriseLookup, new LCU.Graphs.Registry.Enterprises.Provisioning.Environment()
                    {
                        Lookup = buildEnvironmentLookup()
                    });

                    Assert.IsNotNull(mainEnv);

                    var settings = new
                    {
                        AzureAppAuthKey = config["LCU-AZURE-APP-AUTH-KEY"],
                        AzureAppID = config["LCU-AZURE-APP-ID"],
                        AzureLocation = config["LCU-AZURE-LOCATION"],
                        AzureRegion = config["LCU-AZURE-REGION"],
                        AzureSubID = config["LCU-AZURE-SUB-ID"],
                        AzureTenantID = config["LCU-AZURE-TENANT-ID"],
                        EnvironmentLookup = mainEnv.Lookup
                    }.JSONConvert<MetadataModel>();

                    mainEnvSettings = await prvGraph.SaveEnvironmentSettings(mainEnt.EnterpriseLookup, mainEnv.Lookup, new EnvironmentSettings()
                    {
                        Settings = settings
                    });
                }

                if (idGraph != null)
                {
                    var defaultRelyingParty = await loadDefaultRelyingParty();

                    var relyingParty = await idGraph.SaveRelyingParty(defaultRelyingParty, mainEnt.EnterpriseLookup);

                    Assert.IsNotNull(relyingParty);

                    var status = await idGraph.Register(mainEnt.EnterpriseLookup, username, username.ToMD5Hash(), GetType().FullName);

                    Assert.IsNotNull(status);
                    Assert.IsTrue(status);

                    var defaultAccessCard = await loadDefaultAccessCard(relyingParty);

                    var accessCard = await idGraph.SaveAccessCard(defaultAccessCard, mainEnt.EnterpriseLookup, username);

                    Assert.IsNotNull(accessCard);
                }
            }

            Assert.AreNotEqual("3ebd1c0d-22d0-489e-a46f-3260103c8cd7", mainEnt.EnterpriseLookup, "This would blow up everything, so don't do it");
        }
        #endregion
    }
}
