using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using LCU.Graphs.Registry.Enterprises.Edges;
using LCU.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
    public class IDEGraph : LCUGraph
    {
        #region Properties
        #endregion

        #region Constructors
        public IDEGraph(LCUGraphConfig graphConfig, ILogger<IDEGraph> logger)
            : base(graphConfig, logger)
        { }
        #endregion

        #region API Methods
        public virtual async Task<Status> AddSideBarSection(string entLookup, string container, string activityLookup, string section)
        {
            var registry = $"{entLookup}|{container}";

            var activity = await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .FirstOrDefaultAsync();

            if (activity != null)
            {
                activity.Sections = activity.Sections.AddItem(section).Distinct().ToArray();

                activity = await g.V<IDEActivity>(activity.ID)
                    .Update(activity)
                    .FirstOrDefaultAsync();
            }

            return activity != null;
        }

        public virtual async Task<Status> DeleteActivity(string entLookup, string container, string activityLookup)
        {
            var act = await GetActivity(entLookup, container, activityLookup);

            if (act != null)
            {
                await g.V<IDEActivity>(act.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate data flow by that enterprise lookup");
            }
        }

        public virtual async Task<Status> DeleteLCU(string entLookup, string container, string lcuLookup)
        {
            var lcu = await GetLCU(entLookup, container, lcuLookup);

            if (lcu != null)
            {
                await g.V<LCUConfig>(lcu.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate data flow by that enterprise lookup");
            }
        }

        public virtual async Task<Status> DeleteSectionAction(string entLookup, string container, string activityLookup, string section, string action, string group)
        {
            var secAct = await GetSectionAction(entLookup, container, activityLookup, section, action, group);

            if (secAct != null)
            {
                await g.V<SectionAction>(secAct.ID)
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Drop();

                return Status.Success;
            }
            else
            {
                return Status.GeneralError.Clone("Unable to locate data flow by that enterprise lookup");
            }
        }

        public virtual async Task<Status> DeleteSideBarSection(string entLookup, string container, string activityLookup, string section)
        {
            var registry = $"{entLookup}|{container}";

            var activity = await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .FirstOrDefaultAsync();

            if (activity != null)
            {
                activity.Sections = activity.Sections.RemoveItem(section).Distinct().ToArray();

                activity = await g.V<IDEActivity>(activity.ID)
                    .Update(activity)
                    .FirstOrDefaultAsync();
            }

            return activity != null;
        }

        public virtual async Task<IDEContainer> EnsureIDESettings(string entLookup, IDEContainer container)
        {
            var existingContainer = await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container.Container)
                .FirstOrDefaultAsync();

            container.EnterpriseLookup = entLookup;

            container.Registry = entLookup;

            if (existingContainer == null)
            {
                if (container.ID.IsEmpty())
                    container.ID = Guid.NewGuid();

                container = await g.AddV(container).FirstOrDefaultAsync();
            }
            else
            {
                container = await g.V<IDEContainer>(existingContainer.ID)
                    .Update(container)
                    .FirstOrDefaultAsync();
            }

            return container;
        }

        public virtual async Task<IDEActivity> GetActivity(string entLookup, string container, string activityLookup)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<LCUConfig> GetLCU(string entLookup, string container, string lcuLookup)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Manages>()
                .OfType<LCUConfig>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == lcuLookup)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<SectionAction> GetSectionAction(string entLookup, string container, string activityLookup, string section, string action, string group)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .Out<Consumes>()
                .OfType<SectionAction>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Section == section)
                .Where(e => e.Group == group)
                .Where(e => e.Action == action)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<List<IDEActivity>> ListActivities(string entLookup, string container)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();
        }

        public virtual async Task<List<LCUConfig>> ListLCUs(string entLookup, string container)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Manages>()
                .OfType<LCUConfig>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .ToListAsync();
        }

        public virtual async Task<List<SectionAction>> ListSectionActions(string entLookup, string container, string activityLookup, string section)
        {
            var registry = $"{entLookup}|{container}";

            return await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .Out<Consumes>()
                .OfType<SectionAction>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Section == section)
                .ToListAsync();
        }

        public virtual async Task<List<string>> ListSideBarSections(string entLookup, string container, string activityLookup)
        {
            var registry = $"{entLookup}|{container}";

            var activity = await g.V<IDEContainer>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == entLookup)
                .Where(e => e.Container == container)
                .Out<Consumes>()
                .OfType<IDEActivity>()
                .Where(e => e.EnterpriseLookup == entLookup)
                .Where(e => e.Registry == registry)
                .Where(e => e.Lookup == activityLookup)
                .FirstOrDefaultAsync();

            return activity?.Sections.ToList();
        }

        public virtual async Task<IDEActivity> SaveActivity(string entLookup, string container, IDEActivity activity)
        {
            var existingAct = await GetActivity(entLookup, container, activity.Lookup);

            activity.EnterpriseLookup = entLookup;

            activity.Registry = $"{entLookup}|{container}";

            if (existingAct == null)
            {
                if (activity.ID.IsEmpty())
                    activity.ID = Guid.NewGuid();

                activity = await g.AddV(activity).FirstOrDefaultAsync();

                var ide = await g.V<IDEContainer>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == entLookup)
                    .Where(e => e.Container == container)
                    .FirstOrDefaultAsync();

                await ensureEdgeRelationship<Consumes>(ide.ID, activity.ID);

                await ensureEdgeRelationship<Manages>(ide.ID, activity.ID);

                await ensureEdgeRelationship<Owns>(ide.ID, activity.ID);
            }
            else
            {
                activity = await g.V<IDEActivity>(existingAct.ID)
                    .Update(activity)
                    .FirstOrDefaultAsync();
            }

            return activity;
        }

        public virtual async Task<LCUConfig> SaveLCU(string entLookup, string container, LCUConfig lcu)
        {
            var existingLCU = await GetLCU(entLookup, container, lcu.Lookup);

            lcu.EnterpriseLookup = entLookup;

            lcu.Registry = $"{entLookup}|{container}";

            if (existingLCU == null)
            {
                if (lcu.ID.IsEmpty())
                    lcu.ID = Guid.NewGuid();

                lcu = await g.AddV(lcu).FirstOrDefaultAsync();

                var ide = await g.V<IDEContainer>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == entLookup)
                    .Where(e => e.Container == container)
                    .FirstOrDefaultAsync();

                await ensureEdgeRelationship<Consumes>(ide.ID, lcu.ID);

                await ensureEdgeRelationship<Manages>(ide.ID, lcu.ID);

                await ensureEdgeRelationship<Owns>(ide.ID, lcu.ID);
            }
            else
            {
                lcu = await g.V<LCUConfig>(existingLCU.ID)
                    .Update(lcu)
                    .FirstOrDefaultAsync();
            }

            return lcu;
        }

        public virtual async Task<SectionAction> SaveSectionAction(string entLookup, string container, string activityLookup, SectionAction action)
        {
            var existingAct = await GetSectionAction(entLookup, container, activityLookup, action.Section, action.Action, action.Group);

            action.EnterpriseLookup = entLookup;

            action.Registry = $"{entLookup}|{container}";

            if (existingAct == null)
            {
                if (action.ID.IsEmpty())
                    action.ID = Guid.NewGuid();

                action = await g.AddV(action).FirstOrDefaultAsync();

                var ide = await g.V<IDEContainer>()
                    .Where(e => e.EnterpriseLookup == entLookup)
                    .Where(e => e.Registry == entLookup)
                    .Where(e => e.Container == container)
                    .FirstOrDefaultAsync();

                await ensureEdgeRelationship<Consumes>(ide.ID, action.ID);

                await ensureEdgeRelationship<Manages>(ide.ID, action.ID);

                await ensureEdgeRelationship<Owns>(ide.ID, action.ID);
            }
            else
            {
                action = await g.V<SectionAction>(existingAct.ID)
                    .Update(action)
                    .FirstOrDefaultAsync();
            }

            return action;
        }
        #endregion

        #region Helpers
        #endregion
    }
}
