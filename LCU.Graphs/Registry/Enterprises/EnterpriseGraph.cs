﻿using ExRam.Gremlinq.Core;
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
		public virtual async Task<Enterprise> AddHost(string entApiKey, string host)
		{
			return await withG(async (client, g) =>
			{
				var existingEnt = await LoadByHost(host);

				if (existingEnt == null)
				{
					var apiKey = Guid.NewGuid();

					var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
						.Has("PrimaryAPIKey", entApiKey)
						.Has(EntGraphConstants.RegistryName, entApiKey)
						.Property(Cardinality.List, "Hosts", host);

					return await SubmitFirst<Enterprise>(query);
				}
				else
				{
					if (existingEnt.Metadata["Registry"].ToString() != entApiKey)
						throw new Exception("An enterprise with that host already exists.");
					else
						return existingEnt;
				}
			}, entApiKey);
		}

		public virtual async Task<Enterprise> Create(string name, string description, string host)
		{
			var existingEnt = await LoadByHost(host);

			if (existingEnt == null)
			{
				var apiKey = Guid.NewGuid().ToString();

				return await G.AddV(new Enterprise()
				{
					Name = name,
					Hosts = new List<string>() { host },
					Description = description,
					PreventDefaultApplications = false,
					Created = new Audit() { By = "LCU System", Description = typeof(EnterpriseGraph).FullName },
					PrimaryAPIKey = apiKey,
					Registry = apiKey
				}).FirstAsync();
			}
			else
			{
				throw new Exception("An enterprise with that host already exists.");
			}
		}

		public virtual async Task<Status> DeleteEnterprise(string entApiKey)
		{
			return await withG(async (client, g) =>
			{
				var ent = await LoadByPrimaryAPIKey(entApiKey);

				if (ent != null)
				{
					var dataQuery = g.V()
						.Has("EnterpriseAPIKey", entApiKey)
						.Drop();

					await Submit(dataQuery);

					var entQuery = g.V()
						.Has("PrimaryAPIKey", entApiKey)
						.Drop();

					await Submit(entQuery);

					return Status.Success;
				}
				else
				{
					return Status.GeneralError.Clone("Unable to located enterprise by that api key");
				}
			}, entApiKey);
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

		public virtual async Task<List<string>> FindRegisteredHosts(string apiKey, string hostRoot)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Hosts", TextP.EndingWith(hostRoot))
					.Values<string>("Hosts");

				var results = await Submit<string>(query);

				return results.Distinct().ToList();
			}, apiKey);
		}

		public virtual async Task<List<Enterprise>> ListChildEnterprises(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.In(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.EnterpriseVertexName);

				var results = await Submit<Enterprise>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<List<string>> ListRegistrationHosts(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseRegistrationVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Values<string>("Hosts");

				var results = await Submit<string>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<Enterprise> LoadByHost(string host)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName).Has("Hosts", host);

				return await SubmitFirst<Enterprise>(query);
			});
		}

		public virtual async Task<Enterprise> LoadByPrimaryAPIKey(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey);

				return await SubmitFirst<Enterprise>(query);
			}, apiKey);
		}

		public virtual async Task<string> RetrieveThirdPartyData(string apiKey, string key)
		{
			return await withG(async (client, g) =>
			{
				var registry = apiKey;

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("Key", key);

				var tpdResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				return tpdResult?.Metadata["Value"].ToString();
			}, apiKey);
		}

		public virtual async Task<Status> SetThirdPartyData(string apiKey, string key, string value)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("Key", key);

				var tpdResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var setQuery = tpdResult != null ? existingQuery :
					g.AddV(EntGraphConstants.ThirdPartyDataVertexName)
						.Property(EntGraphConstants.RegistryName, apiKey)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
						.Property("Key", key);

				setQuery = setQuery.Property("Value", value);

				tpdResult = await SubmitFirst<BusinessModel<Guid>>(setQuery);

				var entQuery = g.V()
				   .HasLabel(EntGraphConstants.EnterpriseVertexName)
				   .Has(EntGraphConstants.RegistryName, apiKey)
				   .Has("PrimaryAPIKey", apiKey);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				await ensureEdgeRelationships(g, entResult.ID, tpdResult.ID,
					edgeToCheckBuy: EntGraphConstants.OwnsEdgeName, edgesToCreate: new List<string>()
					{
						EntGraphConstants.OwnsEdgeName
					});

				return Status.Success;
			}, apiKey);
		}
		#endregion

		#region Helpers
		#endregion
	}
}
