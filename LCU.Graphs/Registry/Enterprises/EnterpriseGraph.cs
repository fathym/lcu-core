using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises
{
	public class EnterpriseGraph : LCUGraph, IEnterpriseGraph
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
					.Where(e => e.PrimaryAPIKey == entLookup)
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
					Created = new Audit() { By = "LCU System", Description = typeof(EnterpriseGraph).FullName },
					PrimaryAPIKey = entLookup,
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
			var ent = await LoadByPrimaryAPIKey(entLookup);

			if (ent != null)
			{
				var dataQuery = g.V<LCUVertex>()
					.Where(e => e.EnterpriseLookup == entLookup)
					.Has("EnterpriseAPIKey", entLookup)
					.Drop();

				await Submit(dataQuery);

				var entQuery = g.V()
					.Has("PrimaryAPIKey", entLookup)
					.Drop();

				await Submit(entQuery);

				return Status.Success;
			}
			else
			{
				return Status.GeneralError.Clone("Unable to located enterprise by that api key");
			}
		}

		public virtual async Task<bool> DoesHostExist(string host)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName).Has("Hosts", host);

				var entHost = await Submit<Enterprise>(query);

				return entHost != null && entHost.Count > 0;
			});
		}

		public virtual async Task<List<string>> FindRegisteredHosts(string entLookup, string hostRoot)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Hosts", TextP.EndingWith(hostRoot))
					.Values<string>("Hosts");

				var results = await Submit<string>(query);

				return results.Distinct().ToList();
			}, entLookup);
		}

		public virtual async Task<List<Enterprise>> ListChildEnterprises(string entLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.In(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.EnterpriseVertexName);

				var results = await Submit<Enterprise>(query);

				return results.ToList();
			}, entLookup);
		}

		public virtual async Task<List<string>> ListRegistrationHosts(string entLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseRegistrationVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Values<string>("Hosts");

				var results = await Submit<string>(query);

				return results.ToList();
			}, entLookup);
		}

		public virtual async Task<Enterprise> LoadByHost(string host)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName).Has("Hosts", host);

				return await SubmitFirst<Enterprise>(query);
			});
		}

		public virtual async Task<Enterprise> LoadByPrimaryAPIKey(string entLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup);

				return await SubmitFirst<Enterprise>(query);
			}, entLookup);
		}

		public virtual async Task<string> RetrieveThirdPartyData(string entLookup, string key)
		{
			return await withG(async (client, g) =>
			{
				var registry = entLookup;

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("Key", key);

				var tpdResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				return tpdResult?.Metadata["Value"].ToString();
			}, entLookup);
		}

		public virtual async Task<Status> SetThirdPartyData(string entLookup, string key, string value)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("Key", key);

				var tpdResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var setQuery = tpdResult != null ? existingQuery :
					g.AddV(EntGraphConstants.ThirdPartyDataVertexName)
						.Property(EntGraphConstants.RegistryName, entLookup)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
						.Property("Key", key);

				setQuery = setQuery.Property("Value", value);

				tpdResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

				var entQuery = g.V()
				   .HasLabel(EntGraphConstants.EnterpriseVertexName)
				   .Has(EntGraphConstants.RegistryName, entLookup)
				   .Has("PrimaryAPIKey", entLookup);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				await ensureEdgeRelationships(g, entResult.ID, tpdResult.ID,
					edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
					{
						EntGraphConstants.OwnsEdgeName
					});

				return Status.Success;
			}, entLookup);
		}
		#endregion

		#region Helpers
		#endregion
	}
}
