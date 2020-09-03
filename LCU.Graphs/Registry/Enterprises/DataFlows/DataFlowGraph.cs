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
                .OfType<Provisioning.Environment>()
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

            var ent = await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .FirstOrDefaultAsync();

            var env = await g.V<Enterprise>(ent.ID)
                .Out<Consumes>()
                .OfType<Provisioning.Environment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .FirstOrDefaultAsync();

            return await g.V<Provisioning.Environment>(env.ID)
                .Out<Consumes>()
                .OfType<DataFlow>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();

            //var ent = await g.V<Enterprise>()
            //    .Where(e => e.EnterpriseLookup == entLookup)
            //    .Where(e => e.Registry == entLookup)
            //    .Out<Consumes>()
            //    .OfType<LCUEnvironment>()
            //    .Where(e => e.EnterpriseLookup == entLookup)
            //    .Where(e => e.Registry == entLookup)
            //    .Where(e => e.Lookup == envLookup)
            //    .Out<Consumes>()
            //    .OfType<DataFlow>()
            //    .Where(e => e.EnterpriseLookup == entLookup)
            //    .Where(e => e.Registry == registry)
            //    .ToListAsync();
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
                .OfType<Provisioning.Environment>()
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
                .ToArrayAsync();

            setup.Options = await g.V<ModulePack>(setup.Pack.ID)
                .Out<Owns>()
                .OfType<ModuleOption>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToArrayAsync();

            return setup;
        }

        public virtual async Task<DataFlow> SaveDataFlow(string entLookup, string envLookup, DataFlow dataFlow)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            var existingDataFlow = await GetDataFlow(entLookup, envLookup, dataFlow.Lookup);

            if (existingDataFlow == null)
            {
                if (dataFlow.ID.IsEmpty())
                    dataFlow.ID = Guid.NewGuid();

                dataFlow.EnterpriseLookup = entLookup;

                dataFlow.Registry = registry;

                dataFlow = await g.AddV(dataFlow).FirstOrDefaultAsync();
            }
            else if (!dataFlow.ID.IsEmpty())
                dataFlow = await g.V<DataFlow>(dataFlow.ID)
                    .Update(dataFlow)
                    .FirstOrDefaultAsync();
            else
                throw new Exception("A data flow with that lookup already exists");

            var env = await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<Provisioning.Environment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .FirstOrDefaultAsync();

            await ensureEdgeRelationship<Consumes>(env.ID, dataFlow.ID);

            await ensureEdgeRelationship<Manages>(env.ID, dataFlow.ID);

            await ensureEdgeRelationship<Owns>(env.ID, dataFlow.ID);

            return dataFlow;
        }

        public virtual async Task<ModulePackSetup> UnpackModulePack(string entLookup, string envLookup, string dfLookup,
            ModulePackSetup module)
        {
            var registry = $"{entLookup}|{envLookup}|DataFlow";

            var dataFlow = await GetDataFlow(entLookup, envLookup, dfLookup);

            var existingModulePack = await g.V<ModulePack>(module.Pack.ID)
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .FirstOrDefaultAsync();

            if (existingModulePack == null)
            {
                if (module.Pack.ID.IsEmpty())
                    module.Pack.ID = Guid.NewGuid();

                module.Pack.EnterpriseLookup = entLookup;

                module.Pack.Registry = registry;

                module.Pack = await g.AddV(module.Pack).FirstOrDefaultAsync();
            }
            else
                module.Pack = await g.V<ModulePack>(module.Pack.ID)
                    .Update(module.Pack)
                    .FirstOrDefaultAsync();

            await ensureEdgeRelationship<Consumes>(dataFlow.ID, module.Pack.ID);

            await ensureEdgeRelationship<Manages>(dataFlow.ID, module.Pack.ID);

            await ensureEdgeRelationship<Owns>(dataFlow.ID, module.Pack.ID);

            await g.V<ModulePack>(module.Pack.ID)
                .Out<Owns>()
                .OfType<ModuleOption>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Drop();

            await module.Options.Each(async option =>
            {
                await unpackModuleOption(registry, entLookup, dataFlow.ID, module.Pack, option);
            });

            await module.Displays.Each(async display =>
            {
                await unpackModuleDisplay(registry, entLookup, dataFlow.ID, module.Pack, display);
            });

            return module;
        }
        #endregion

        #region Helpers
        protected virtual async Task<ModuleOption> unpackModuleOption(string registry,
            string entLookup, Guid dataFlowId, ModulePack modulePack, ModuleOption option)
        {
            var existingModuleOption = await g.V<ModulePack>(modulePack.ID)
                .Out<Owns>()
                .OfType<ModuleOption>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.ModuleType == option.ModuleType)
                .FirstOrDefaultAsync();

            if (existingModuleOption == null)
            {
                if (option.ID.IsEmpty())
                    option.ID = Guid.NewGuid();

                option.EnterpriseLookup = entLookup;

                option.Registry = registry;

                option = await g.AddV(option).FirstOrDefaultAsync();
            }
            else
                option = await g.V<ModuleOption>(option.ID)
                    .Update(option)
                    .FirstOrDefaultAsync();

            await ensureEdgeRelationship<Consumes>(modulePack.ID, option.ID);

            await ensureEdgeRelationship<Manages>(modulePack.ID, option.ID);

            await ensureEdgeRelationship<Owns>(modulePack.ID, option.ID);

            return option;
        }

        protected virtual async Task<ModuleDisplay> unpackModuleDisplay(string registry,
            string entLookup, Guid dataFlowId, ModulePack modulePack, ModuleDisplay display)
        {
            var existingModuleDisplay = await g.V<ModulePack>(modulePack.ID)
                .Out<Owns>()
                .OfType<ModuleDisplay>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.ModuleType == display.ModuleType)
                .FirstOrDefaultAsync();

            if (existingModuleDisplay == null)
            {
                if (display.ID.IsEmpty())
                    display.ID = Guid.NewGuid();

                display.EnterpriseLookup = entLookup;

                display.Registry = registry;

                display = await g.AddV(display).FirstOrDefaultAsync();
            }
            else
                display = await g.V<ModuleDisplay>(display.ID)
                    .Update(display)
                    .FirstOrDefaultAsync();

            await ensureEdgeRelationship<Consumes>(modulePack.ID, display.ID);

            await ensureEdgeRelationship<Manages>(modulePack.ID, display.ID);

            await ensureEdgeRelationship<Owns>(modulePack.ID, display.ID);

            return display;
        }
        #endregion
    }
}
