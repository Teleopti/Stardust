using System;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using ManagerTest.Attributes;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[ManagerOperationTests]
	[TestFixture]
	public class ManagerOperationsTest : DatabaseTest
	{
		public ManagerController ManagerController;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository NodeRepository;
		public NodeManager NodeManager;
		public FakeHttpSender HttpSender;
		public ManagerConfiguration ManagerConfiguration;
		public TestHelper TestHelper;
		public IJobManager JobManager;

		private WorkerNode _workerNode;

		[OneTimeSetUp]
		public void TextFixtureSetUp()
		{
			_workerNode = new WorkerNode
			{
				Url = new Uri("http://localhost:9050/")
			};
		}

		[Test]
		public void ShouldBeAbleToCancelJobOnNode()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			NodeRepository.AddWorkerNode(_workerNode);

			JobRepository.AddItemToJobQueue(jobQueueItem);

			JobRepository.AssignJobToWorkerNode();

			HttpSender.CallToWorkerNodes.Clear();
			ManagerController.CancelJobByJobId(jobQueueItem.JobId);

			HttpSender.CallToWorkerNodes.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendJobWhenNoJobsInQueue()
		{
			NodeRepository.AddWorkerNode(_workerNode);
			HttpSender.CallToWorkerNodes.Clear();
			JobRepository.AssignJobToWorkerNode();
			HttpSender.CallToWorkerNodes.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldBeAbleToPersistNewJobQueueItem()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			ManagerController.AddItemToJobQueue(jobQueueItem);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetUniqueJobIdWhilePersistingJob()
		{
			var jobQueueItem1 = new JobQueueItem
			{
				Name = "Name1 Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};
			var jobQueueItem2 = new JobQueueItem
			{
				Name = "Name2 Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			ManagerController.AddItemToJobQueue(jobQueueItem1);
			ManagerController.AddItemToJobQueue(jobQueueItem2);

			var queuedItems = JobRepository.GetAllItemsInJobQueue();

			queuedItems[0].Should().Not.Be.EqualTo(queuedItems[1]);
		}


		[Test]
		public void ShouldNotBeAbleToPersist_WhenInvalidJobQueueItem()
		{
			ManagerController.AddItemToJobQueue(new JobQueueItem
			{
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = ""
			});

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAQueuedJobIfCancelled()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			NodeRepository.AddWorkerNode(_workerNode);

			JobRepository.AddItemToJobQueue(jobQueueItem);

			JobRepository.AssignJobToWorkerNode();

			ManagerController.CancelJobByJobId(jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(0);
		}

		
		[Test] 
		public void ShouldSendTheJobToAnotherNodeIfFirstReturnsConflict()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};

			var workerNode2 = new WorkerNode
			{
				Url = new Uri("http://localhost:9051/")
			};
			
			NodeRepository.AddWorkerNode(_workerNode);
			Thread.Sleep(TimeSpan.FromSeconds(1));
			NodeRepository.AddWorkerNode(workerNode2);
			ThisNodeIsBusy(workerNode2.Url);

			ManagerController.AddItemToJobQueue(jobQueueItem);
			
			while (true)
			{
				if (JobRepository.GetAllJobs().Count == 1)
					break;
				
				Thread.Sleep(500);
			}
			
			JobRepository.GetAllJobs().First().SentToWorkerNodeUri.Should().Be.EqualTo(_workerNode.Url.ToString());
		}

		[Test]
		public void ShouldReturnIdOfPersistedJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				CreatedBy = "Created By Test",
				Serialized = "Serialized Test",
				Type = "Type Test"
			};
			
			IHttpActionResult actionResult =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			var okNegotiatedContentResult = actionResult as OkNegotiatedContentResult<Guid>;
			var jobId = okNegotiatedContentResult.Content;

			jobId.Satisfy(guid => guid != Guid.Empty);
		}

		[Test]
		public void ShouldAssignANewIdForANewQueueItem()
		{
			Guid oldGuid = Guid.NewGuid();
			var jobQueueItem = new JobQueueItem
			{
				JobId = oldGuid,
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "CreatedBy"
			};

			ManagerController.AddItemToJobQueue(jobQueueItem);

			var queuedItems = JobRepository.GetAllItemsInJobQueue();

			queuedItems[0].JobId.Should().Not.Be.EqualTo(oldGuid);
		}

		[Test]
		public void HeartbeatTimestampShouldBeChangedOnHeartbeat()
		{
			NodeRepository.AddWorkerNode(_workerNode);

			var beforeHeartbeat = TestHelper.GetAllNodes().First(x => x.Url.Equals(_workerNode.Url)).Heartbeat;
			Thread.Sleep(TimeSpan.FromSeconds(1));

			NodeManager.WorkerNodeRegisterHeartbeat(_workerNode.Url.ToString());

			var afterHeartbeat = TestHelper.GetAllNodes().First(x => x.Url.Equals(_workerNode.Url)).Heartbeat;

			Assert.IsTrue(beforeHeartbeat < afterHeartbeat);
		}

		private void ThisNodeIsBusy(Uri url)
		{
			HttpSender.BusyNodesUrl.Add(url.ToString());
		}
	}
}