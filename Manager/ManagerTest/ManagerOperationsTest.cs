using System;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
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
	[ManagerOperationTests]
	[TestFixture]
	public class ManagerOperationsTest : DatabaseTest
	{
		public ManagerController ManagerController;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository NodeRepository;
		public NodeManager NodeManager;
		public FakeHttpSender HttpSender;
		
		private WorkerNode _workerNode;

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

			JobRepository.AssignJobToWorkerNode(HttpSender, useThisWorkerNodeUri: null);

			HttpSender.CallToWorkerNodes.Clear();
			ManagerController.CancelJobByJobId(jobQueueItem.JobId);

			HttpSender.CallToWorkerNodes.Count().Should().Be.EqualTo(1);
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

			JobRepository.AssignJobToWorkerNode(HttpSender, useThisWorkerNodeUri: null);

			ManagerController.CancelJobByJobId(jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(0);
		}


		//We should maybe redesign this to not sleep and wait for the _jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri:null); to be done in the Controller.
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
				Id = Guid.NewGuid(),
				Url = new Uri("http://localhost:9051/")
			};

			NodeRepository.AddWorkerNode(_workerNode);

			ThisNodeIsBusy(_workerNode.Url);

			NodeRepository.AddWorkerNode(workerNode2);

			ManagerController.AddItemToJobQueue(jobQueueItem);

			var jobs = JobRepository.GetAllJobs();

			int numberOftries = 0;

			while (!jobs.Any())
			{
				numberOftries ++;

				jobs = JobRepository.GetAllJobs();

				Thread.Sleep(TimeSpan.FromSeconds(3));

				if (numberOftries == 10)
				{
					break;
				}
			}

			JobRepository.GetAllJobs()[0].SentToWorkerNodeUri.Should().Be.EqualTo(workerNode2.Url.ToString());
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
			
			var beforeHeartbeat = NodeRepository.GetWorkerNodeByNodeId(_workerNode.Url).Heartbeat;

			Thread.Sleep(TimeSpan.FromSeconds(1));

			NodeManager.WorkerNodeRegisterHeartbeat(_workerNode.Url.ToString());
			
			var afterHeartbeat = NodeRepository.GetWorkerNodeByNodeId(_workerNode.Url).Heartbeat;

			Assert.IsTrue(beforeHeartbeat < afterHeartbeat);
		}

		private void ThisNodeIsBusy(Uri url)
		{
			HttpSender.BusyNodesUrl.Add(url.ToString());
		}
	}
}