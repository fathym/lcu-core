using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
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
		public EnterpriseGraph(LCUGraphConfig config)
			: base(config)
		{
			ListProperties.Add("Hosts");
		}
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
						.Has("Registry", entApiKey)
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
			});
		}

		public virtual async Task<Enterprise> Create(string name, string description, string host)
		{
			return await withG(async (client, g) =>
			{
				var existingEnt = await LoadByHost(host);

				if (existingEnt == null)
				{
					var apiKey = Guid.NewGuid();

					var query = g.AddV(EntGraphConstants.EnterpriseVertexName)
						.Property(Cardinality.List, "Hosts", host)
						.Property("Name", name)
						.Property("Description", description)
						.Property("PreventDefaultApplications", false)
						.Property("PrimaryAPIKey", apiKey)
						.Property("Registry", apiKey);

					return await SubmitFirst<Enterprise>(query);
				}
				else
				{
					throw new Exception("An enterprise with that host already exists.");
				}
			});
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

		public virtual async Task<List<string>> ListRegistrationHosts(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseRegistrationVertexName)
					.Has("Registry", apiKey)
					.Values<string>("Hosts");

				var results = await Submit<string>(query);

				return results.ToList();
			});
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
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey);

				return await SubmitFirst<Enterprise>(query);
			});
		}

		public virtual async Task<string> RetrieveThirdPartyData(string apiKey, string key)
		{
			return await withG(async (client, g) =>
			{
				var registry = apiKey;

				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has("Registry", apiKey)
					.Has("Key", key);

				var tpdResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var tpdResult = tpdResults.FirstOrDefault();

				return tpdResult?.Metadata["Value"].ToString();
			});
		}

		public virtual async Task<Status> SetThirdPartyData(string apiKey, string key, string value)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V()
					.HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ThirdPartyDataVertexName)
					.Has("Registry", apiKey)
					.Has("Key", key);

				var tpdResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var tpdResult = tpdResults.FirstOrDefault();

				var setQuery = tpdResult != null ? existingQuery :
					g.AddV(EntGraphConstants.ThirdPartyDataVertexName)
						.Property("Registry", apiKey)
						.Property("Key", key);

				setQuery = setQuery.Property("Value", value);

				tpdResults = await Submit<BusinessModel<Guid>>(setQuery);

				tpdResult = tpdResults.FirstOrDefault();

				var entQuery = g.V()
				   .HasLabel(EntGraphConstants.EnterpriseVertexName)
				   .Has("Registry", apiKey)
				   .Has("PrimaryAPIKey", apiKey);

				var entResults = await Submit<Enterprise>(entQuery);

				var entResult = entResults.FirstOrDefault();

				var edgeResults = await Submit<BusinessModel<Guid>>(g.V(entResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(tpdResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
							g.V(entResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(tpdResult.ID)),
						};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return Status.Success;
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
