﻿using System;
using System.Configuration;
using System.Threading;
using ManagerTest.Database;
using NUnit.Framework;
using Stardust.Manager;
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
			WorkerNodeRepository = 
				new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, 
										 new RetryPolicyProvider());
		}
		

		[Test]
		public void HeartbeatTimestampShouldBeChangedOnHeartbeat()
		{
			var node = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1,
				Heartbeat = DateTime.UtcNow,
				Alive = true
			};
			WorkerNodeRepository.AddWorkerNode(node);

			node = WorkerNodeRepository.GetWorkerNodeByNodeId(_nodeUri1);
			var beforeHeartbeat = node.Heartbeat;

			Thread.Sleep(TimeSpan.FromSeconds(2)); 

			WorkerNodeRepository.RegisterHeartbeat(_nodeUri1.ToString(), true);

			node = WorkerNodeRepository.GetWorkerNodeByNodeId(_nodeUri1);
			var afterHeartbeat = node.Heartbeat;

			Assert.IsTrue(beforeHeartbeat < afterHeartbeat);
		}
	}
}