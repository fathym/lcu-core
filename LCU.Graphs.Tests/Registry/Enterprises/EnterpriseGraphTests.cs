using LCU.Graphs.Registry.Enterprises;
using LCU.Testing.Graphs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Tests.Registry.Enterprises
{
	[TestClass]
	public class EnterpriseGraphTests : GenericGraphTests
	{
		#region Fields
		#endregion

		#region Constructors
		public EnterpriseGraphTests()
			: base()
		{ }
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
		}
		#endregion

		#region API Methods
		[TestMethod]
		public async Task Create()
		{
			var rand = Guid.NewGuid();

			var name = $"{GetType().FullName}-Test-{rand}";

			var ent = await entGraph.Create(name, "A description", mainHost);

			Assert.IsNotNull(ent);

			addEntForCleanup(ent.EnterpriseLookup);

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));
		}

		[TestMethod]
		public async Task CreateDuplicates()
		{
			var rand = Guid.NewGuid();

			var name = $"{GetType().FullName}-Test-{rand}";
			
			var ent = await entGraph.Create(name, "A description", mainHost);

			Assert.IsNotNull(ent);

			addEntForCleanup(ent.EnterpriseLookup);

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));

			await Assert.ThrowsExceptionAsync<Exception>(async () =>
			{
				var ent2 = await entGraph.Create(name, "A description", mainHost);

				addEntForCleanup(ent2.EnterpriseLookup);
			});
		}

		[TestMethod]
		public async Task CreateDelete()
		{
			var rand = Guid.NewGuid();

			var name = $"{GetType().FullName}-Test-{rand}";

			var ent = await entGraph.Create(name, "A description", mainHost);

			Assert.IsNotNull(ent);

			addEntForCleanup(ent.EnterpriseLookup);

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));

			var status = await entGraph.DeleteEnterprise(ent.EnterpriseLookup);

			Assert.IsNotNull(status);
			Assert.IsTrue(status);
		}

		[TestMethod]
		public async Task EnterpriseWhitelabelingChecks()
		{
			var rand = Guid.NewGuid();

			var name = $"{GetType().FullName}-Test-{rand}";

			var childEnts = await entGraph.ListChildEnterprises(parentEntLookup);

			Assert.IsNotNull(childEnts);
			Assert.IsFalse(childEnts.IsNullOrEmpty());

			var regHosts = await entGraph.ListRegistrationHosts(parentEntLookup);

			Assert.IsNotNull(regHosts);
			Assert.IsFalse(regHosts.IsNullOrEmpty());
			Assert.IsTrue(regHosts.Contains("fathym-int.com"));
		}

		[TestMethod]
		public async Task HostChecks()
		{
			var rand = Guid.NewGuid();

			var name = $"{GetType().FullName}-Test-{rand}";

			var ent = await entGraph.Create(name, "A description", mainHost);

			Assert.IsNotNull(ent);

			addEntForCleanup(ent.EnterpriseLookup);

			ent = await entGraph.AddHost(ent.EnterpriseLookup, $"added-{mainHost}");

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));
			Assert.IsTrue(ent.Hosts.Contains($"added-{mainHost}"));

			ent = await entGraph.AddHost(ent.EnterpriseLookup, $"added2-{mainHost}");

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));
			Assert.IsTrue(ent.Hosts.Contains($"added-{mainHost}"));
			Assert.IsTrue(ent.Hosts.Contains($"added2-{mainHost}"));

			var status = await entGraph.DoesHostExist(mainHost);

			Assert.IsNotNull(status);
			Assert.IsTrue(status);

			var registeredHosts = await entGraph.FindRegisteredHosts(hostRoot);

			Assert.IsNotNull(registeredHosts);
			Assert.IsFalse(registeredHosts.IsNullOrEmpty());
			Assert.IsTrue(registeredHosts.Contains(mainHost));
			Assert.IsTrue(registeredHosts.Contains($"added-{mainHost}"));
			Assert.IsTrue(registeredHosts.Contains($"added2-{mainHost}"));

			ent = await entGraph.LoadByHost($"added2-{mainHost}");

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));
			Assert.IsTrue(ent.Hosts.Contains($"added-{mainHost}"));
			Assert.IsTrue(ent.Hosts.Contains($"added2-{mainHost}"));

			ent = await entGraph.LoadByLookup(ent.EnterpriseLookup);

			Assert.AreNotEqual(Guid.Empty, ent.ID);
			Assert.AreEqual(name, ent.Name);
			Assert.AreEqual("A description", ent.Description);
			Assert.IsTrue(ent.Hosts.Contains(mainHost));
			Assert.IsTrue(ent.Hosts.Contains($"added-{mainHost}"));
			Assert.IsTrue(ent.Hosts.Contains($"added2-{mainHost}"));
		}
		#endregion
	}
}
