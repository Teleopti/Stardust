﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using log4net.Config;
using ManagerTest.Attributes;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Stardust.Manager.Policies;

namespace ManagerTest
{
	[TestFixture, JobTests]
	public class JobManagerTest : DatabaseTest
	{
		[TearDown]
		public void TearDown()
		{
			JobManager.Dispose();
		}

		public JobManager JobManager;
		public NodeManager NodeManager;
		public IHttpSender HttpSender;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository WorkerNodeRepository;
		public JobRepositoryCommandExecuter JobRepositoryCommandExecuter;
		public ManagerConfiguration ManagerConfiguration;
		public TestHelper TestHelper;

		private WorkerNode _workerNode;
		private WorkerNode _workerNode2;
		private FakeHttpSender FakeHttpSender => (FakeHttpSender) HttpSender;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			_workerNode = new WorkerNode
			{
				Url = new Uri("http://localhost:9050/")
			};
			_workerNode2 = new WorkerNode
			{
				Url = new Uri("http://localhost:9051/")
			};

			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
		}

		[Test]
		public void ShouldAddANodeOnInit()
		{
			NodeManager.AddWorkerNode(_workerNode.Url);

			TestHelper.GetAllNodes()
				.First()
				.Url.Should()
				.Be.EqualTo(_workerNode.Url.ToString());
		}

		[Test]
		public void ShouldBeAbleToSendAQueuedJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToSendAQueuedJob_WhenCheckPolicy_AndHaveEnoughWorkerNode()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				Policy = HalfNodesAffinityPolicy.PolicyName
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);
			WorkerNodeRepository.AddWorkerNode(_workerNode2);

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotBeAbleToSendAQueuedJob_WhenCheckPolicy_AndHaveNotEnoughWorkerNode()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				Policy = HalfNodesAffinityPolicy.PolicyName
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
			JobRepository.GetAllJobs().Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotBeAbleToSendAnotherQueuedJob_WhenCheckPolicy_AndHaveNotEnoughWorkerNode()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				Policy = HalfNodesAffinityPolicy.PolicyName
			};

			var jobQueueItem2 = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				Policy = HalfNodesAffinityPolicy.PolicyName
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);
			WorkerNodeRepository.AddWorkerNode(_workerNode2);

			JobManager.AddItemToJobQueue(jobQueueItem);
			JobManager.AddItemToJobQueue(jobQueueItem2);

			JobManager.AssignJobToWorkerNodes();

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToAddToJobQueue()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToCancelJobIfStarted()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			var job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);

			JobManager.CancelJobByJobId(jobQueueItem.JobId);

			job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Result.StartsWith("cancel", StringComparison.InvariantCultureIgnoreCase));

			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldContainJobManager()
		{
			JobManager.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDoNothing_WhenNoWorkerNodeExists_AndJobQueueHasItems()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldDoNothing_WhenNoWorkerNodeExists_AndJobQueueIsEmpty()
		{
			JobManager.AssignJobToWorkerNodes();

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceOnInit()
		{
			NodeManager.AddWorkerNode(_workerNode.Url);
			NodeManager.AddWorkerNode(_workerNode.Url);

			TestHelper.GetAllNodes().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDeleteJobFromJobQueueIfNotAssignedToWorkerNode()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.CancelJobByJobId(jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
		}


		[Test]
		public void ShouldBeAbleToRequeueDeadJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			WorkerNodeRepository.AddWorkerNode(_workerNode);

			JobManager.AddItemToJobQueue(jobQueueItem);

			JobManager.AssignJobToWorkerNodes();

			var job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);
			job.Satisfy(job1 => job1.Ended == null);

			NodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(job.SentToWorkerNodeUri);

			JobRepository.GetAllJobs().Count.Should().Be(0);
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}

		[Test]
		public void ShouldReturnAvailableNodes()
		{
			NodeManager.AddWorkerNode(new Uri("http://localhost:9051/"));

			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			JobManager.AddItemToJobQueue(jobQueueItem);
			JobManager.AssignJobToWorkerNodes();

			TestHelper.AddDeadNode("http://localhost:9052/");
			NodeManager.AddWorkerNode(new Uri("http://localhost:9053/"));

			List<Uri> nodes;
			using (var sqlConnection = new SqlConnection(ManagerConfiguration.ConnectionString))
			{
				sqlConnection.Open();
				nodes = JobRepositoryCommandExecuter.SelectAllAvailableWorkerNodes(sqlConnection);
			}
			nodes.Count.Should().Be.EqualTo(1);
			nodes.First().Should().Be.EqualTo("http://localhost:9053/");
		}

		[Test]
		public void ShouldJustWarnIfAnotherManagerAlreadySentJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "CreatedBy",
				Created = DateTime.Now
			};
			NodeManager.AddWorkerNode(_workerNode.Url);

			using (var sqlConnection = new SqlConnection(ManagerConfiguration.ConnectionString))
			{
				sqlConnection.Open();
				JobRepositoryCommandExecuter.InsertIntoJobQueue(jobQueueItem, sqlConnection);
				JobRepositoryCommandExecuter.InsertIntoJob(jobQueueItem, "aNode", sqlConnection);
			}
			
			JobManager.AssignJobToWorkerNodes();
			var assertSQL = "SELECT COUNT(*) FROM Stardust.Logging WHERE [Level] = 'WARN' AND [Message] like '%It was already assigned, probably by another Manager.%'";
			using (var sqlConnection = new SqlConnection(ManagerConfiguration.ConnectionString))
			{
				using (var getAllJobsCommand = new SqlCommand(assertSQL, sqlConnection))
				{
					sqlConnection.Open();
					var ret = getAllJobsCommand.ExecuteScalar();
					ret.Should().Be.EqualTo(1);
				}
			}
		}
	}
}
