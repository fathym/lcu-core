using ExRam.Gremlinq.Core;
using Fathym;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.Edges;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
    public class DataFlowGraph : LCUGraph
    {
        #region Properties
        #endregion

        #region Constructors
        public DataFlowGraph(LCUGraphConfig graphConfig, ILogger<DataFlowGraph> logger)
            : base(graphConfig, logger)
        { }
        #endregion

        #region API Methods
        public virtual async Task<Status> DeleteDataFlow(string entLookup, string envLookup, string dfLookup)
        {
            var df = await GetDataFlow(entLookup, envLookup, dfLookup);

            if (df != null)
            {
                await g.V<DataFlow>(df.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate data flow by that enterprise lookup");
            }
        }

        public virtual async Task<DataFlow> GetDataFlow(string entLookup, string envLookup, string dfLookup)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Consumes>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Out<Consumes>()
                .OfType<DataFlow>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == dfLookup)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<List<DataFlow>> ListDataFlows(string entLookup, string envLookup)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Consumes>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Out<Consumes>()
                .OfType<DataFlow>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();
        }

        public virtual async Task<ModulePackSetup> LoadModulePackSetup(string entLookup, string envLookup,
            string dfLookup, string mdlPckLookup)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            var setup = new ModulePackSetup();

            setup.Pack = await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Out<Owns>()
                .OfType<DataFlow>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == dfLookup)
                .Out<Owns>()
                .OfType<ModulePack>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == dfLookup)
                .FirstOrDefaultAsync();

            setup.Displays = await g.V<ModulePack>(setup.Pack.ID)
                .Out<Owns>()
                .OfType<ModuleDisplay>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();

            setup.Options = await g.V<ModulePack>(setup.Pack.ID)
                .Out<Owns>()
                .OfType<ModuleOption>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();

            return setup;
        }

        public virtual async Task<DataFlow> SaveDataFlow(string entLookup, string envLookup, DataFlow dataFlow)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            var existingDataFlow = await g.V<DataFlow>(dataFlow.ID)
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .FirstOrDefaultAsync();

            if (existingDataFlow == null)
            {
                if (dataFlow.ID.IsEmpty())
                    dataFlow.ID = Guid.NewGuid();

                dataFlow.EnterpriseLookup = entLookup;

                dataFlow.Registry = registry;

                dataFlow = await g.AddV(dataFlow).FirstOrDefaultAsync();
            }
            else
                dataFlow = await g.V<DataFlow>(dataFlow.ID)
                    .Update(dataFlow)
                    .FirstOrDefaultAsync();

            var ent = await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .FirstOrDefaultAsync();

            var env = await g.V<Enterprise>(ent.ID)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .FirstOrDefaultAsync();

            await ensureEdgeRelationship<Provides>(app.ID, dafApp.ID);

            return dafApp;
            return await withG(async (client, g) =>
            {
                var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                    .Has(EntGraphConstants.RegistryName, entLookup)
                    .Has("PrimaryAPIKey", entLookup)
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
                    .Has(EntGraphConstants.RegistryName, entLookup)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has("Lookup", envLookup);

                var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

                var existingQuery = envQuery
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.DataFlowVertexName)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Lookup", dataFlow.Lookup);

                var existingResult = loadDataFlowFromMetadata(await SubmitFirst<MetadataModel>(existingQuery));

                var query = existingResult == null ?
                    g.AddV(EntGraphConstants.DataFlowVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup) : existingQuery;

                query = query
                    .Property("Name", dataFlow.Name ?? "")
                    .Property("Description", dataFlow.Description ?? "")
                    .Property("Lookup", dataFlow.Lookup ?? "")
                    .Property("Output", dataFlow.Output ?? new DataFlowOutput()
                    {
                        Modules = new List<Module>(),
                        Streams = new List<ModuleStream>()
                    });

                query.SideEffect(__.Properties<string>("ModulePacks").Drop());

                dataFlow.ModulePacks.Each(mp => query = query.Property(Cardinality.List, "ModulePacks", mp));

                var dfResult = loadDataFlowFromMetadata(await SubmitFirst<MetadataModel>(query));

                await ensureEdgeRelationships(g, envResult.ID, dfResult.ID);

                return dfResult;
            }, entLookup);
        }

        public virtual async Task<ModulePack> UnpackModulePack(string entLookup, string envLookup, string dfLookup,
            ModulePackSetup module)
        {
            return await withG(async (client, g) =>
            {
                var registry = $"{entLookup}|{envLookup}|DataFlow";

                var dfQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                    .Has(EntGraphConstants.RegistryName, entLookup)
                    .Has("PrimaryAPIKey", entLookup)
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
                    .Has(EntGraphConstants.RegistryName, entLookup)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has("Lookup", envLookup)
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.DataFlowVertexName)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Lookup", dfLookup);

                var dfResult = await SubmitFirst<DataFlow>(dfQuery);

                var existingQuery = dfQuery
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.ModulePackVertexName)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has("Lookup", module.Pack.Lookup);

                var existingResult = await SubmitFirst<ModulePack>(existingQuery);

                var query = existingResult == null ?
                    g.AddV(EntGraphConstants.ModulePackVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup) : existingQuery;

                query = query
                    .Property("Name", module.Pack.Name ?? "")
                    .Property("Description", module.Pack.Description ?? "")
                    .Property("Lookup", module.Pack.Lookup ?? "");

                var mpResult = await SubmitFirst<ModulePack>(query);

                await ensureEdgeRelationships(g, dfResult.ID, mpResult.ID);

                //	TODO:  Drop any previous Module Options for the Enterprise and reload new
                var dropOptionsQuery = existingQuery
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.ModuleOptionVertexName)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Drop();

                await module.Options.Each(async option =>
                {
                    await unpackModuleOption(g, registry, entLookup, dfResult.ID, mpResult, option);
                });

                await module.Displays.Each(async display =>
                {
                    await unpackModuleDisplay(g, registry, entLookup, dfResult.ID, mpResult, display);
                });

                return mpResult;
            }, entLookup);
        }
        #endregion

        #region Helpers
        protected virtual async Task<ModuleOption> unpackModuleOption(GraphTraversalSource g, string registry,
            string entLookup, Guid dataFlowId, ModulePack modulePack, ModuleOption option)
        {
            var existingQuery = g.V(modulePack.ID)
                .Out(EntGraphConstants.OwnsEdgeName)
                .HasLabel(EntGraphConstants.ModuleOptionVertexName)
                .Has(EntGraphConstants.RegistryName, registry)
                .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                .Has("ModuleType", option.ModuleType);

            var existingResult = await SubmitFirst<ModuleOption>(existingQuery);

            var query = existingResult == null ?
                g.AddV(EntGraphConstants.ModuleOptionVertexName)
                .Property(EntGraphConstants.RegistryName, registry)
                .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                .Property("Active", true)
                .Property("Visible", true) : existingQuery;

            query = query
                .Property("ControlType", option.ControlType)
                .Property("Description", option.Description ?? "")
                .Property("IncomingConnectionLimit", option.IncomingConnectionLimit)
                .Property("ModuleType", option.ModuleType ?? "")
                .Property("Name", option.Name ?? "")
                .Property("OutgoingConnectionLimit", option.OutgoingConnectionLimit);

            query.SideEffect(__.Properties<string>("IncomingConnectionTypes").Drop());

            option.IncomingConnectionTypes.Each(ict =>
                query = query.Property(Cardinality.List, "IncomingConnectionTypes", ict));

            query.SideEffect(__.Properties<string>("OutgoingConnectionTypes").Drop());

            option.OutgoingConnectionTypes.Each(oct =>
                query = query.Property(Cardinality.List, "OutgoingConnectionTypes", oct));

            var moResult = await SubmitFirst<ModuleOption>(query);

            await ensureEdgeRelationships(g, dataFlowId, moResult.ID);

            return moResult;
        }

        protected virtual async Task<ModuleDisplay> unpackModuleDisplay(GraphTraversalSource g, string registry,
            string entLookup, Guid dataFlowId, ModulePack modulePack, ModuleDisplay display)
        {
            var existingQuery = g.V(modulePack.ID)
                .Out(EntGraphConstants.OwnsEdgeName)
                .HasLabel(EntGraphConstants.ModuleDisplayVertexName)
                .Has(EntGraphConstants.RegistryName, registry)
                .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                .Has("ModuleType", display.ModuleType);

            var existingResult = await SubmitFirst<ModuleDisplay>(existingQuery);

            //	In case the Module Display has been updated, only create new, and don't update if it exists
            if (existingResult == null)
            {
                var query = g.AddV(EntGraphConstants.ModuleDisplayVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Property("Category", display.Category)
                    .Property("Element", display.Element)
                    .Property("Height", display.Height)
                    .Property("Icon", display.Icon)
                    .Property("ModuleType", display.ModuleType)
                    .Property("Shape", display.Shape)
                    .Property("Width", display.Width);

                var msResult = await SubmitFirst<ModuleDisplay>(query);

                await ensureEdgeRelationships(g, dataFlowId, msResult.ID);

                return msResult;
            }
            else
                return existingResult;
        }
        #endregion
    }
}
