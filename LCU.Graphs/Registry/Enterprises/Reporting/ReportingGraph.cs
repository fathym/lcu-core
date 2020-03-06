using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Reporting
{
	public class ReportingGraph : LCUGraph, IReportingGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public ReportingGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			
		}
		#endregion

		#region API Methods
		public virtual async Task<PowerBIConfig> GetPowerBIConfig(string clientId)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(ReportingGraphConstants.PowerBIConfigVertexName)
				.Has(ReportingGraphConstants.PowerBIConfigClientIdPropertyName, clientId);

				var existing = await SubmitFirst<PowerBIConfig>(existingQuery);

				if (existing != null)
				{
					return existing;
				}
				else
				{
					throw new Exception("A power BI config with that clientId does not exist.");
				}
			});
		}

		public virtual async Task<Status> SavePowerBIConfig(PowerBIConfig config)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(ReportingGraphConstants.PowerBIConfigVertexName)
				.Has(ReportingGraphConstants.PowerBIConfigClientIdPropertyName, config.ClientId);

				var existing = await SubmitFirst<PowerBIConfig>(existingQuery);

				if (existing == null)
				{
					var apiKey = Guid.NewGuid();

					var query = g.AddV(ReportingGraphConstants.PowerBIConfigVertexName)
						.Property("AuthorityUrl", config.AuthorityUrl)
						.Property("ClientId", config.ClientId)
						.Property("GroupId", config.GroupId)
						.Property("Password", config.Password)
						.Property("PrimaryAPIKey", apiKey)
						.Property("Username", config.Username)
						.Property("ResourceUrl", config.ResourceUrl)
						.Property(ReportingGraphConstants.RegistryName, apiKey);

					await SubmitFirst<PowerBIConfig>(query);

					return Status.Success;
				}
				else
				{
					throw new Exception("An power BI config with that clientId already exists.");
				}
			});
		}

		#endregion

		#region Helpers
		#endregion
	}
}
