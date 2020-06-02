using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using LCU.Graphs.Registry.Enterprises.Edges;
using Microsoft.Extensions.Logging;
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
			var existingEnt = await LoadByHost(host);

			if (existingEnt == null)
			{
				return await g.V<Enterprise>()
					.Where(e => e.EnterpriseLookup == entLookup)
					.Where(e => e.Registry == entLookup)
					.Property(e => e.Hosts, new List<string>() { host })
					.FirstAsync();
			}
			else
			{
				if (existingEnt.Metadata["Registry"].ToString() != entLookup)
					throw new Exception("An enterprise with that host already exists.");
				else
					return existingEnt;
			}
		}

		public virtual async Task<Enterprise> Create(string name, string description, string host)
		{
			var existingEnt = await LoadByHost(host);

			if (existingEnt == null)
			{
				var entLookup = Guid.NewGuid().ToString();

				return await g.AddV(new Enterprise()
				{
					Name = name,
					Hosts = new List<string>() { host },
					Description = description,
					PreventDefaultApplications = false,
					Created = buildAudit(),
					EnterpriseLookup = entLookup,
					Registry = entLookup
				}).FirstAsync();
			}
			else
			{
				throw new Exception("An enterprise with that host already exists.");
			}
		}

		public virtual async Task<Status> DeleteEnterprise(string entLookup)
		{
			var ent = await LoadByLookup(entLookup);

			if (ent != null)
			{
				//	TODO:  We should be archiving the Enterprise records somewhere for potential reimport?

				await g.V<LCUVertex>()
					.Where(e => e.EnterpriseLookup == entLookup)
					.Drop();

				return Status.Success;
			}
			else
			{
				return Status.GeneralError.Clone("Unable to located enterprise by that api key");
			}
		}

		public virtual async Task<bool> DoesHostExist(string host)
		{
			var ent = await g.V<Enterprise>()
				.Where(e => e.Hosts.Contains(host))
				.FirstAsync();

			return ent != null;
		}

		public virtual async Task<List<string>> FindRegisteredHosts(string entLookup, string hostRoot)
		{
			var hosts = await g.V<Enterprise>()
				.Where(e => e.Hosts.Any(h => h.EndsWith(hostRoot)))
				.Values(e => e.Hosts);

			return hosts.SelectMany(hs => hs).Distinct().ToList();
		}

		public virtual async Task<List<Enterprise>> ListChildEnterprises(string entLookup)
		{
			return await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Owns>()
				.OfType<DefaultApplications>()
				.In<Offers>()
				.OfType<Enterprise>()
				.ToListAsync();
		}

		public virtual async Task<List<string>> ListRegistrationHosts(string entLookup)
		{
			var hosts = await g.V<EnterpriseRegistration>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Values(e => e.Hosts)
				.FirstAsync();

			return hosts;
		}

		public virtual async Task<Enterprise> LoadByHost(string host)
		{
			return await g.V<Enterprise>()
				.Where(e => e.Hosts.Contains(host))
				.FirstAsync();
		}

		public virtual async Task<Enterprise> LoadByLookup(string entLookup)
		{
			return await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.FirstAsync();
		}

		public virtual async Task<string> RetrieveThirdPartyData(string entLookup, string key)
		{
			return await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Owns>()
				.OfType<ThirdPartyIdentifier>()
				.Where(e => e.Registry == entLookup)
				.Where(e => e.Key == key)
				.Values(e => e.Value)
				.FirstAsync();
		}

		public virtual async Task<Status> SetThirdPartyData(string entLookup, string key, string value)
		{
			var ent = await LoadByLookup(entLookup);

			var tpi = await g.V<Enterprise>(ent.ID)
				.Out<Owns>()
				.OfType<ThirdPartyIdentifier>()
				.Where(e => e.Registry == entLookup)
				.Where(e => e.Key == key)
				.FirstAsync();

			if (tpi == null)
			{
				tpi = await g.AddV(new ThirdPartyIdentifier()
				{
					Key = key,
					Value = value,
					EnterpriseLookup = entLookup,
					Registry = entLookup,
					Created = buildAudit()
				})
				.FirstAsync();

				await g.V(ent.ID)
					.AddE<Owns>()
					.To(__ => __.V(tpi.ID))
					.FirstAsync();
			}
			else
			{
				tpi = await g.V<ThirdPartyIdentifier>(tpi.ID)
					.Property(e => e.Value, value)
					.Property(e => e.Modified, buildAudit())
					.FirstAsync();
			}

			return Status.Success;
		}
		#endregion

		#region Helpers
		#endregion
	}
}
