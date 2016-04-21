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
	public class JobManagerTestsNewVersion : DatabaseTest
	{
		[TearDown]
		public void TearDown()
		{
			JobManager.Dispose();
		}

		public JobManagerNewVersion JobManager;
		public NodeManager NodeManager;
		public IHttpSender HttpSender;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository WorkerNodeRepository;

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");
		private readonly Uri _nodeUri2 = new Uri("http://localhost:9051/");
		private readonly Uri _nodeUri3 = new Uri("http://localhost:9052/");

		private FakeHttpSender FakeHttpSender
		{
			get { return (FakeHttpSender) HttpSender; }
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
			NodeManager.AddIfNeeded(_nodeUri1);

			WorkerNodeRepository.GetAllWorkerNodes()
				.First()
				.Url.Should()
				.Be.EqualTo(_nodeUri1.ToString());
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
			JobManager.AssignJobToWorkerNode();
			FakeHttpSender.CallToWorkerNodes.Count().Should().Be(2);

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
		public void ShouldJustDeleteJobIfNotStarted()
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
	}
}