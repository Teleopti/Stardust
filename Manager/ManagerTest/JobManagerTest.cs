using System;
using System.IO;
using System.Linq;
using log4net.Config;
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

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");
		private readonly Uri _nodeUri2 = new Uri("http://localhost:9051/");
		private readonly Uri _nodeUri3 = new Uri("http://localhost:9052/");

		private FakeHttpSender FakeHttpSender
		{
			get { return (FakeHttpSender)HttpSender; }
		}

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[Test]
		public void ShouldAddANodeOnInit()
		{
			NodeManager.AddWorkerNode(_nodeUri1);

			WorkerNodeRepository.GetAllWorkerNodes()
				.First()
				.Url.Should()
				.Be.EqualTo(_nodeUri1.ToString());
		}


		[Test]
		public void ShouldBeAbleToAddAJob()
		{
			//------------------------------------------
			// Add worker node.
			//------------------------------------------
			var workerNode = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			};
			WorkerNodeRepository.AddWorkerNode(workerNode);

			//------------------------------------------
			// Create job queue item.
			//------------------------------------------			
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};
			//----------------------------------------------
			// Add item to job queue.
			//----------------------------------------------
			JobManager.AddItemToJobQueue(jobQueueItem);

			//----------------------------------------------
			// Validate.
			//----------------------------------------------
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);

			//----------------------------------------------
			// Assign to worker node.
			//----------------------------------------------
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);

			//----------------------------------------------
			// Validate.
			//----------------------------------------------
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}


		[Test]
		public void ShouldBeAbleToDeleteFromJobQueue()
		{
			//------------------------------------------
			// Create job queue item.
			//------------------------------------------			
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			//----------------------------------------------
			// Add item to job queue.
			//----------------------------------------------
			JobManager.AddItemToJobQueue(jobQueueItem);

			//----------------------------------------------
			// Validate.
			//----------------------------------------------
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);

			//----------------------------------------------
			// Delete item from job queue.
			//----------------------------------------------
			JobManager.DeleteJobQueueItemByJobId(jobQueueItem.JobId);

			//----------------------------------------------
			// Validate.
			//----------------------------------------------
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);

		}

		[Test]
		public void ShouldBeAbleToAddToJobQueue()
		{
			//------------------------------------------
			// Create job queue item.
			//------------------------------------------			
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			//----------------------------------------------
			// Add item to job queue.
			//----------------------------------------------
			JobManager.AddItemToJobQueue(jobQueueItem);

			//----------------------------------------------
			// Validate.
			//----------------------------------------------
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToCancelJobIfStarted()
		{
			//------------------------------------------
			// Add worker node.
			//------------------------------------------
			var workerNode = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			};
			WorkerNodeRepository.AddWorkerNode(workerNode);

			//------------------------------------------
			// Create job queue item.
			//------------------------------------------			
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			//----------------------------------------------
			// Add item to job queue.
			//----------------------------------------------
			JobManager.AddItemToJobQueue(jobQueueItem);

			//----------------------------------------------
			// Assign job to worker node.
			//----------------------------------------------
			FakeHttpSender.CallToWorkerNodes.Clear();
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			FakeHttpSender.CallToWorkerNodes.Count().Should().Be(3);

			//----------------------------------------------
			// Check that job started.
			//----------------------------------------------
			var job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);

			//----------------------------------------------
			// Try to cancel the job.
			//----------------------------------------------
			FakeHttpSender.CallToWorkerNodes.Clear();
			JobManager.CancelJobByJobId(jobQueueItem.JobId);
			FakeHttpSender.CallToWorkerNodes.Count.Should().Be.EqualTo(1);

			//----------------------------------------------
			// Check that job is canceling or has canceled.
			//----------------------------------------------
			job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Result.StartsWith("cancel", StringComparison.InvariantCultureIgnoreCase));

			var deletedJobQueueItemExists =
				JobManager.DoesJobQueueItemExists(jobQueueItem.JobId);

			deletedJobQueueItemExists.Should().Be.False();

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

			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldDoNothing_WhenNoWorkerNodeExists_AndJobQueueIsEmpty()
		{
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);

			FakeHttpSender.CallToWorkerNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceInInit()
		{
			NodeManager.AddWorkerNode(_nodeUri1);
			NodeManager.AddWorkerNode(_nodeUri1);

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

			var deletedJobQueueItemExists =
				JobManager.DoesJobQueueItemExists(jobQueueItem.JobId);

			deletedJobQueueItemExists.Should().Be.False();
		}


		[Test]
		public void ShouldBeAbleToRequeueDeadJob()
		{
			//------------------------------------------
			// Add worker node.
			//------------------------------------------
			var workerNode = new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			};
			WorkerNodeRepository.AddWorkerNode(workerNode);

			//------------------------------------------
			// Create job queue item.
			//------------------------------------------			
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "ShouldBeAbleToRequeueDeadJob",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			//----------------------------------------------
			// Add item to job queue.
			//----------------------------------------------
			JobManager.AddItemToJobQueue(jobQueueItem);

			//----------------------------------------------
			// Assign job to worker node.
			//----------------------------------------------
			FakeHttpSender.CallToWorkerNodes.Clear();
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			FakeHttpSender.CallToWorkerNodes.Count().Should().Be(3);

			//----------------------------------------------
			// Check that job started.
			//----------------------------------------------
			var job = JobManager.GetJobByJobId(jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);
			job.Satisfy(job1 => job1.Ended == null);

			NodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(new Uri(job.SentToWorkerNodeUri),
												keepJobDetailsIfExists: false);

			JobRepository.GetAllJobs().Count.Should().Be(0);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}


	}
}