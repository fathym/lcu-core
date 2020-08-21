using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.Edges;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
    public class ProvisioningGraph : LCUGraph
    {
        #region Properties
        #endregion

        #region Constructors
        public ProvisioningGraph(LCUGraphConfig graphConfig, ILogger<ProvisioningGraph> logger)
            : base(graphConfig, logger)
        { }
        #endregion

        #region API Methods
        public virtual async Task<LCUEnvironment> GetEnvironment(string entLookup, string lookup)
        {
            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Consumes>()
                .OfType<LCUEnvironment>()
                .Where(e => e.Lookup == lookup)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<LCUEnvironmentSettings> GetEnvironmentSettings(string entLookup, string envLookup)
        {
            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Consumes>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Out<Consumes>()
                .OfType<LCUEnvironmentSettings>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<SourceControl> GetSourceControl(string entLookup, string envLookup)
        {
            var registry = $"{entLookup}|{envLookup}";

            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Out<Owns>()
                .OfType<SourceControl>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<List<LCUEnvironment>> ListEnvironments(string entLookup)
        {
            return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .ToListAsync();
        }

        public virtual async Task<Status> RemoveEnvironment(string entLookup, string envLookup)
        {
            await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .Drop();

            return Status.Success;
        }

        public virtual async Task<Status> RemoveEnvironmentSettings(string entLookup, string envLookup)
        {
            await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<LCUEnvironment>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Lookup == envLookup)
                .OfType<LCUEnvironmentSettings>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Drop();

            return Status.Success;
        }

        public virtual async Task<LCUEnvironment> SaveEnvironment(string entLookup, LCUEnvironment env)
        {
            var existingEnv = await GetEnvironment(entLookup, env.Lookup);

            env.EnterpriseLookup = entLookup;

            env.Registry = entLookup;

            if (existingEnv == null)
            {
                if (env.ID.IsEmpty())
                    env.ID = Guid.NewGuid();

                env = await g.AddV(env).FirstOrDefaultAsync();

                var ent = await g.V<Enterprise>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == entLookup)
                    .FirstOrDefaultAsync();

                await ensureEdgeRelationship<Consumes>(ent.ID, env.ID);

                await ensureEdgeRelationship<Manages>(ent.ID, env.ID);

                await ensureEdgeRelationship<Owns>(ent.ID, env.ID);
            }
            else
            {
                env = await g.V<LCUEnvironment>(existingEnv.ID)
                    .Update(env)
                    .FirstOrDefaultAsync();
            }

            return env;
        }

        public virtual async Task<LCUEnvironmentSettings> SaveEnvironmentSettings(string entLookup, string envLookup, LCUEnvironmentSettings settings)
        {
            var existingSettings = await GetEnvironmentSettings(entLookup, envLookup);

            settings.EnterpriseLookup = entLookup;

            settings.Registry = entLookup;

            if (existingSettings == null)
            {
                if (settings.ID.IsEmpty())
                    settings.ID = Guid.NewGuid();

                settings = await g.AddV(settings).FirstOrDefaultAsync();

                var env = await GetEnvironment(entLookup, envLookup);

                await ensureEdgeRelationship<Consumes>(env.ID, settings.ID);

                await ensureEdgeRelationship<Manages>(env.ID, settings.ID);

                await ensureEdgeRelationship<Owns>(env.ID, settings.ID);
            }
            else
            {
                settings = await g.V<LCUEnvironmentSettings>(existingSettings.ID)
                    .Update(settings)
                    .FirstOrDefaultAsync();
            }

            return settings;
        }

        public virtual async Task<SourceControl> SaveSourceControl(string entLookup, string envLookup, SourceControl sc)
        {
            var registry = $"{entLookup}|{envLookup}";

            var existingSC = await GetSourceControl(entLookup, envLookup);

            sc.EnterpriseLookup = entLookup;

            sc.Registry = entLookup;

            if (existingSC == null)
            {
                if (sc.ID.IsEmpty())
                    sc.ID = Guid.NewGuid();

                sc = await g.AddV(sc).FirstOrDefaultAsync();

                var env = await GetEnvironment(entLookup, envLookup);

                await ensureEdgeRelationship<Consumes>(env.ID, sc.ID);

                await ensureEdgeRelationship<Manages>(env.ID, sc.ID);

                await ensureEdgeRelationship<Owns>(env.ID, sc.ID);
            }
            else
            {
                sc = await g.V<SourceControl>(existingSC.ID)
                    .Update(sc)
                    .FirstOrDefaultAsync();
            }

            return sc;
        }
        #endregion

        #region Helpers
        #endregion
    }
}
