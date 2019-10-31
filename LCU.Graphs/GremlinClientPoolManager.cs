using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Graphs
{
	public class GremlinClientPoolManager
	{
        #region Fields
        protected ApplicationProfileManager appProfileMgr;

		protected readonly IDictionary<string, Tuple<GremlinClient, DateTime>> clients;

		protected LCUGraphConfig config;

        protected static string defaultClientId;

		protected GremlinServer server;
		#endregion

		#region Properties
		public virtual int ClientCount
		{
			get { return clients.Count; }
		}
		#endregion

		#region Constructors
		public GremlinClientPoolManager(LCUGraphConfig config, ApplicationProfileManager appProfileMgr)
		{
            defaultClientId = "DefaultClient";

            clients = new Dictionary<string, Tuple<GremlinClient, DateTime>>();

			this.config = config;

            this.appProfileMgr = appProfileMgr;

            var username = $"/dbs/{config.Database}/colls/{config.Graph}";

            server = CreateServer(config, username);

            Task.Run(() => cleanupClients());
		}
		#endregion

		#region API Methods
		public virtual GremlinClient CreateClient(GremlinServer server, int poolSize, int maxInProcessPerConnection)
		{
            ConnectionPoolSettings poolSettings = new ConnectionPoolSettings();

            poolSettings.PoolSize = poolSize;

            poolSettings.MaxInProcessPerConnection = maxInProcessPerConnection;

            var client = new GremlinClient(server, new GraphSON2Reader(), new GraphSON2Writer(), 
                GremlinClient.GraphSON2MimeType, poolSettings);

            return client;
		}

		public virtual GremlinServer CreateServer(LCUGraphConfig config, string username)
		{
			return new GremlinServer(config.Host, config.Port, config.EnableSSL, username, config.APIKey);
		}

		public virtual GremlinClient LoadClient(string clientId = "")
		{
            if (clientId.IsNullOrEmpty())
                clientId = defaultClientId;

            GremlinClient client = null;

            var appProfile = appProfileMgr.LoadApplicationProfile(clientId);

            if (clients.ContainsKey(clientId))
            {
                client = clients[clientId].Item1;

                var expireTime = clients[clientId].Item2;

                if (DateTime.Now > expireTime)
                {
                    client.Dispose();

                    client = null;

                    clients.Remove(clientId);
                }
            }

            if (client == null)
            {
                client = CreateClient(server, appProfile.DatabaseClientPoolSize, appProfile.DatabaseClientMaxPoolConnections);

                clients.Add(clientId, new Tuple<GremlinClient, DateTime>(client, DateTime.Now.AddMinutes(appProfile.DatabaseClientTTLMinutes)));
            }

			return client;
		}
		#endregion

		#region Helpers
		protected void cleanupClients()
        {
            while (1 == 1)
            {
                var toRemove = new List<string>();

                clients.Keys.Each(
                    (key) =>
                    {
                        if (DateTime.Now > clients[key].Item2)
                            toRemove.Add(key);
                    });

                toRemove.ForEach(
                    (remove) =>
                    {
                        clients[remove].Item1.Dispose();

                        clients[remove] = null;

                        clients.Remove(remove);
                    });

                Thread.Sleep(1000);
            }
        }
		#endregion
	}
}
