using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Testing.Graphs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises.Apps
{
	[TestClass]
	public class ApplicationGraphTests : GenericGraphTests
	{
		#region Fields
		protected readonly ApplicationGraph appGraph;
		#endregion

		#region Constructors
		public ApplicationGraphTests()
			: base()
		{
			appGraph = new ApplicationGraph(graphConfig, createLogger<ApplicationGraph>());
		}
		#endregion

		#region Life Cycle
		[TestCleanup]
		public virtual async Task Cleanup()
		{
			await cleanupEnterprises();
		}

		[TestInitialize]
		public virtual async Task Initialize()
		{
			await setupMainEnt(entGraph, appGraph);
		}
		#endregion

		#region API Methods
		[TestMethod]
		public async Task Save()
		{
			var rand = Guid.NewGuid();

			var accessRight = "LCU.Test.Everything";

			var license = "lcu";

			var expected = new Application()
			{
				Name = $"{GetType().FullName}-Test-{rand}",
				Description = "A description",
				Container = "test-data-app",
				IsPrivate = true,
				IsReadOnly = true,
				PathRegex = "*",
				Priority = 100,
				QueryRegex = "*",
				Registry = mainEnt.EnterpriseLookup,
				UserAgentRegex = "*",
				EnterpriseLookup = mainEnt.EnterpriseLookup,
				AccessRights = new string[] { accessRight },
				Hosts = new string[] { mainHost },
				Licenses = new string[] { license }
			};

			var app = await appGraph.Save(expected);

			Assert.IsNotNull(app);
			Assert.AreNotEqual(Guid.Empty, app.ID);
			Assert.AreEqual(expected.Container, app.Container);
			Assert.AreEqual(expected.Description, app.Description);
			Assert.AreEqual(expected.EnterpriseLookup, app.EnterpriseLookup);
			Assert.IsTrue(expected.IsPrivate);
			Assert.IsTrue(expected.IsReadOnly);
			Assert.AreEqual(expected.Name, app.Name);
			Assert.AreEqual(expected.Label, app.Label);
			Assert.AreEqual(expected.PathRegex, app.PathRegex);
			Assert.AreEqual(expected.Priority, app.Priority);
			Assert.AreEqual(expected.QueryRegex, app.QueryRegex);
			Assert.AreEqual(expected.Registry, app.Registry);
			Assert.AreEqual(expected.UserAgentRegex, app.UserAgentRegex);
			Assert.IsTrue(app.AccessRights.Contains(accessRight));
			Assert.AreEqual(1, app.AccessRights.Length);
			Assert.IsTrue(app.Hosts.Contains(mainHost));
			Assert.AreEqual(1, app.Hosts.Length);
			Assert.IsTrue(app.Licenses.Contains(license));
			Assert.AreEqual(1, app.Licenses.Length);
		}
		#endregion
	}
}
