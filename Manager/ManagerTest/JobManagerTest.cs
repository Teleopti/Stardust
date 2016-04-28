﻿using System;
using System.Linq;
using ManagerTest.Attributes;
using ManagerTest.Database;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

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
		
		private WorkerNode _workerNode;


		private FakeHttpSender FakeHttpSender
		{
			get { return (FakeHttpSender)HttpSender; }
		}

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			_workerNode = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = new Uri("http://localhost:9050/")
			};
		}

		[Test]
		public void ShouldAddANodeOnInit()
		{
			NodeManager.AddWorkerNode(_workerNode.Url);

			WorkerNodeRepository.GetAllWorkerNodes()
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

			JobManager.AssignJobToWorkerNode();

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToDeleteFromJobQueue()
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
			
			JobManager.DeleteJobQueueItemByJobId(jobQueueItem.JobId);
			
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
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
			
			JobManager.AssignJobToWorkerNode();
			
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

			JobManager.AssignJobToWorkerNode();

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldDoNothing_WhenNoWorkerNodeExists_AndJobQueueIsEmpty()
		{
			JobManager.AssignJobToWorkerNode();

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceOnInit()
		{
			NodeManager.AddWorkerNode(_workerNode.Url);
			NodeManager.AddWorkerNode(_workerNode.Url);

			WorkerNodeRepository.GetAllWorkerNodes().Count.Should().Be.EqualTo(1);
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
			
			JobManager.AssignJobToWorkerNode();
			
			var job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);
			job.Satisfy(job1 => job1.Ended == null);

			NodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(new Uri(job.SentToWorkerNodeUri));

			JobRepository.GetAllJobs().Count.Should().Be(0);
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}
	}
}