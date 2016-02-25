using System;
using System.Threading;
using ManagerTest.Database;
using NUnit.Framework;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[TestFixture, JobTests]
	class WorkerNodeRepositoryTests : DatabaseTest
	{
		public IWorkerNodeRepository WorkerNodeRepository { get; set; }

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");


		[Test, Ignore]
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

			WorkerNodeRepository.RegisterHeartbeat(_nodeUri1);

			Thread.Sleep(TimeSpan.FromSeconds(2));  //Do another way?

			node = WorkerNodeRepository.LoadWorkerNode(_nodeUri1);
			var afterHeartbeat = node.Heartbeat;

			Assert.IsTrue(beforeHeartbeat < afterHeartbeat);
		}
	}
}