using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using System.Collections.Generic;

namespace LCU.Graphs
{
	public class GremlinClientPoolManager
	{
		#region Fields
		protected readonly List<GremlinClient> clients;

		protected LCUGraphConfig config;

		protected int lastClientIndex;

		protected GremlinServer server;
		#endregion

		#region Properties
		public virtual int ClientCount
		{
			get { return clients.Count; }
		}
		#endregion

		#region Constructors
		public GremlinClientPoolManager(LCUGraphConfig config, int clientCount)
		{
			clients = new List<GremlinClient>();

			this.config = config;

			lastClientIndex = -1;

			setupGremlinClients(clientCount);
		}
		#endregion

		#region API Methods
		public virtual GremlinClient CreateClient(GremlinServer server)
		{
			return new GremlinClient(server, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);
		}

		public virtual GremlinServer CreateServer(LCUGraphConfig config, string username)
		{
			return new GremlinServer(config.Host, config.Port, config.EnableSSL, username, config.APIKey);
		}

		public virtual GremlinClient LoadClient()
		{
			var client = clients[lastClientIndex++];

			if (lastClientIndex >= ClientCount)
				lastClientIndex = -1;

			return client;
		}
		#endregion

		#region Helpers
		protected virtual void setupGremlinClients(int clientCount)
		{
			var username = $"/dbs/{config.Database}/colls/{config.Graph}";

			server = CreateServer(config, username);

			for (var i = 0; i < clientCount; i++)
				clients.Add(CreateClient(server));
		}
		#endregion
	}
}
