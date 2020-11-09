using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using LCU.Graphs.Registry.Enterprises.Edges;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises
{
    public class EnterpriseGraph : LCUGraph
    {
        #region Properties
        #endregion

        #region Constructors
        public EnterpriseGraph(LCUGraphConfig graphConfig, ILogger<EnterpriseGraph> logger)
            : base(graphConfig, logger)
        { }
        #endregion

        #region API Methods
        public virtual async Task<Enterprise> AddHost(string entLookup, string host)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var existingEnt = await LoadByHost(host);

                if (existingEnt == null)
                {
                    return await g.V<Enterprise>()
                        .Where(e => e.EnterpriseLookup == entLookup)
                        .Where(e => e.Registry == entLookup)
                        .Property(e => e.Hosts, new string[] { host })
                        .FirstOrDefaultAsync();
                }
                else
                {
                    if (existingEnt.EnterpriseLookup != entLookup)
                        throw new Exception("An enterprise with that host already exists.");
                    else
                        return existingEnt;
                }
            });
        }

        public virtual async Task<Enterprise> Create(string name, string description, string host)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var existingEnt = await LoadByHost(host);

                if (existingEnt == null)
                {
                    var entLookup = Guid.NewGuid().ToString();

                    return await g.AddV(new Enterprise()
                    {
                        ID = Guid.NewGuid(),
                        Name = name,
                        Hosts = new string[] { host },
                        Description = description,
                        PreventDefaultApplications = false,
                        EnterpriseLookup = entLookup,
                        Registry = entLookup
                    }).FirstOrDefaultAsync();
                }
                else
                {
                    throw new Exception("An enterprise with that host already exists.");
                }
            });
        }

        public virtual async Task<Status> DeleteEnterprise(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                if (entLookup == "3ebd1c0d-22d0-489e-a46f-3260103c8cd7")
                    throw new Exception("This would blow up everything, so don't do it");

                var ent = await LoadByLookup(entLookup);

                if (ent != null && ent.EnterpriseLookup != "3ebd1c0d-22d0-489e-a46f-3260103c8cd7")
                {
                    //	TODO:  We should be archiving the Enterprise records somewhere for potential reimport?

                    await g.V<LCUVertex>()
                        .Where(e => e.EnterpriseLookup == entLookup)
                        .Drop();

                    return Status.Success;
                }
                else
                {
                    return Status.GeneralError.Clone("Unable to located enterprise by that enterprise lookup");
                }
            });
        }

        public virtual async Task<bool> DoesHostExist(string host)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var ent = await g.V<Enterprise>()
                .Where(e => e.Hosts.Contains(host))
                .FirstOrDefaultAsync();

                return ent != null;
            });
        }

        public virtual async Task<List<string>> FindRegisteredHosts(string hostRoot)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var hosts = await g.V<Enterprise>()
                .Values(e => e.Hosts);

                hosts = hosts.Where(h => h.EndsWith(hostRoot)).ToArray();

                return hosts.Distinct().ToList();
            });
        }

        public virtual async Task<Enterprise> GetParentEnterprise(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Offers>()
                .OfType<DefaultApplications>()
                .In<Owns>()
                .OfType<Enterprise>()
                .FirstOrDefaultAsync();
            });
        }

        public virtual async Task<List<Enterprise>> ListChildEnterprises(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<DefaultApplications>()
                .In<Offers>()
                .OfType<Enterprise>()
                .ToListAsync();
            });
        }

        public virtual async Task<List<string>> ListRegistrationHosts(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var hosts = await g.V<EnterpriseRegistration>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Values(e => e.Hosts);

                return hosts.ToList();
            });
        }

        public virtual async Task<Enterprise> LoadByHost(string host)
        {
            return await withCommonGraphBoundary(async () =>
            {
                return await g.V<Enterprise>()
                        .Where(e => e.Hosts.Contains(host))
                        .FirstOrDefaultAsync();
            });
        }

        public virtual async Task<Enterprise> LoadByLookup(string entLookup)
        {
            return await withCommonGraphBoundary(async () =>
            {
                return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .FirstOrDefaultAsync();
            });
        }

        public virtual async Task<string> RetrieveThirdPartyData(string entLookup, string key)
        {
            return await withCommonGraphBoundary(async () =>
            {
                return await g.V<Enterprise>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Out<Owns>()
                .OfType<ThirdPartyIdentifier>()
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Key == key)
                .Values(e => e.Value)
                .FirstOrDefaultAsync();
            });
        }

        public virtual async Task<Status> SetThirdPartyData(string entLookup, string key, string value)
        {
            return await withCommonGraphBoundary(async () =>
            {
                var ent = await LoadByLookup(entLookup);

                var tpi = await g.V<Enterprise>(ent.ID)
                    .Out<Owns>()
                    .OfType<ThirdPartyIdentifier>()
                    .Where(e => e.Registry == entLookup)
                    .Where(e => e.Key == key)
                    .FirstOrDefaultAsync();

                if (tpi == null)
                {
                    tpi = await g.AddV(new ThirdPartyIdentifier()
                    {
                        ID = Guid.NewGuid(),
                        Key = key,
                        Value = value,
                        EnterpriseLookup = entLookup,
                        Registry = entLookup,
                        //Created = buildAudit()
                    })
                    .FirstOrDefaultAsync();

                    await EnsureEdgeRelationship<Owns>(ent.ID, tpi.ID);
                }
                else
                {
                    tpi = await g.V<ThirdPartyIdentifier>(tpi.ID)
                        .Property(e => e.Value, value)
                        //.Property(e => e.Modified, buildAudit())
                        .FirstOrDefaultAsync();
                }

                return Status.Success;
            });
        }
        #endregion

        #region Helpers
        #endregion
    }
}
