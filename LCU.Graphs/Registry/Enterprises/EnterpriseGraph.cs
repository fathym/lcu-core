using Gremlin.Net.Process.Traversal;
using System;
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
		public virtual async Task<Enterprise> Create(string name, string description, string host)
		{
			return await withG(async (client, g) =>
			{
				var apiKey = Guid.NewGuid();

				var query = g.AddV(EntGraphConstants.EnterpriseVertexName)
					.Property(Cardinality.List, "Hosts", host, new object[] { })
					.Property("Name", name)
					.Property("Description", description)
					.Property("PreventDefaultApplications", false)
					.Property("PrimaryAPIKey", apiKey)
					.Property("Registry", apiKey);

				var results = await Submit<Enterprise>(query);

				return results.FirstOrDefault();
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

		public virtual async Task<Enterprise> LoadByHost(string host)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName).Has("Hosts", host);

				var results = await Submit<Enterprise>(query);

				return results.FirstOrDefault();
			});
		}

		public virtual async Task<Enterprise> LoadByPrimaryAPIKey(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey);

				var results = await Submit<Enterprise>(query);

				return results.FirstOrDefault();
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
