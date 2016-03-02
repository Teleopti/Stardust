using System;
using System.Configuration;
using System.Threading;
using Autofac;
using ManagerTest.Database;
using NUnit.Framework;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[TestFixture]
	class WorkerNodeRepositoryTests : DatabaseTest
	{
		public WorkerNodeRepository WorkerNodeRepository { get; set; }

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			WorkerNodeRepository = new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString);
		}
		

		[Test]
		public void HeartbeatTimestampShouldBeChangedOnHeartbeat()
		{
			var node = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1,
				Heartbeat = DateTime.Now,
				Alive = "true"
			};
			WorkerNodeRepository.Add(node);

			node = WorkerNodeRepository.LoadWorkerNode(_nodeUri1);
			var beforeHeartbeat = node.Heartbeat;

			Thread.Sleep(TimeSpan.FromSeconds(1)); // to create a difference between heartbeats

			WorkerNodeRepository.RegisterHeartbeat(_nodeUri1.ToString());

			Thread.Sleep(TimeSpan.FromSeconds(1));  // to let the task that writes heartbeat finish

			node = WorkerNodeRepository.LoadWorkerNode(_nodeUri1);
			var afterHeartbeat = node.Heartbeat;

			Assert.IsTrue(beforeHeartbeat < afterHeartbeat);
		}
	}
}