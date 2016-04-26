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

		private JobQueueItem _jobQueueItem;
		private WorkerNode _workerNode;


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
			_jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

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
			WorkerNodeRepository.AddWorkerNode(_workerNode);
			
			JobManager.AddItemToJobQueue(_jobQueueItem);

			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
			JobRepository.GetAllJobs().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToDeleteFromJobQueue()
		{		
			JobManager.AddItemToJobQueue(_jobQueueItem);
			
			JobManager.DeleteJobQueueItemByJobId(_jobQueueItem.JobId);
			
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
		}

		[Test]
		public void ShouldBeAbleToAddToJobQueue()
		{		
			JobManager.AddItemToJobQueue(_jobQueueItem);
			
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}

		[Test]
		public void ShouldBeAbleToCancelJobIfStarted()
		{
			WorkerNodeRepository.AddWorkerNode(_workerNode);
		
			JobManager.AddItemToJobQueue(_jobQueueItem);
			
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			
			var job = JobManager.GetJobByJobId(_jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);
			
			JobManager.CancelJobByJobId(_jobQueueItem.JobId);
			
			job = JobManager.GetJobByJobId(_jobQueueItem.JobId);
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
			JobManager.AddItemToJobQueue(_jobQueueItem);

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
		public void ShouldNotAddSameNodeTwiceOnInit()
		{
			NodeManager.AddWorkerNode(_workerNode.Url);
			NodeManager.AddWorkerNode(_workerNode.Url);

			WorkerNodeRepository.GetAllWorkerNodes().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDeleteJobFromJobQueueIfNotAssignedToWorkerNode()
		{
			JobManager.AddItemToJobQueue(_jobQueueItem);

			JobManager.CancelJobByJobId(_jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(0);
		}


		[Test]
		public void ShouldBeAbleToRequeueDeadJob()
		{
			WorkerNodeRepository.AddWorkerNode(_workerNode);
		
			JobManager.AddItemToJobQueue(_jobQueueItem);
			
			JobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			
			var job = JobManager.GetJobByJobId(_jobQueueItem.JobId);
			job.Satisfy(job1 => job1.Started != null);
			job.Satisfy(job1 => job1.Ended == null);

			NodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(new Uri(job.SentToWorkerNodeUri),
												keepJobDetailsIfExists: false);

			JobRepository.GetAllJobs().Count.Should().Be(0);
			JobRepository.GetAllItemsInJobQueue().Count.Should().Be(1);
		}


	}
}