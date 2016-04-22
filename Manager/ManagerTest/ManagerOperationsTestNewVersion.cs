using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
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
	[ManagerOperationTests]
	[TestFixture]
	public class ManagerOperationsTestNewVersion
	{
		[SetUp]
		public void Setup()
		{
			_databaseHelper.TryClearDatabase();
		}

		public ManagerController ManagerController;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository NodeRepository;
		public INodeManager NodeManager;
		public ManagerConfiguration ManagerConfiguration;
		public FakeHttpSender HttpSender;

		private DatabaseHelper _databaseHelper;

		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");
		private readonly Uri _nodeUri2 = new Uri("http://localhost:9051/");

		private void ThisNodeIsBusy(string url)
		{
			HttpSender.BusyNodesUrl.Add(url);
		}

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			_databaseHelper = new DatabaseHelper();
			_databaseHelper.Create();
#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[Test]
		public void ResetJobsOnFalseClaimOnHeartBeatIfItsFree()
		{
			var jobId = Guid.NewGuid();

			var userName = "ManagerOperationsTestNewVersion";

			var job = new JobQueueItem
			{
				JobId = jobId,
				Name = "job",
				CreatedBy = userName,
				Serialized = "Fake Serialized",
				Type = "Fake Type"
			};

			JobRepository.AddItemToJobQueue(job);

			NodeRepository.AddWorkerNode(new WorkerNode
			{
				Url = _nodeUri1
			});

			JobRepository.AssignJobToWorkerNode(HttpSender);

			ManagerController.WorkerNodeRegisterHeartbeat(_nodeUri1);

			HttpSender.CallToWorkerNodes.First().Contains(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldBeAbleToAcknowledgeWhenJobIsReceived()
		{
			var job = new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};

			var result =
				ManagerController.AddItemToJobQueue(job);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToCancelJobOnNode()
		{
			NodeRepository.AddWorkerNode(new WorkerNode
			{
				Url = _nodeUri1
			});

			NodeRepository.AddWorkerNode(new WorkerNode
			{
				Url = _nodeUri2
			});

			ManagerController.WorkerNodeRegisterHeartbeat(_nodeUri1);
			ManagerController.WorkerNodeRegisterHeartbeat(_nodeUri2);

			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Serialized = "Serialized Test",
				Name = "Name Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};
			JobRepository.AddItemToJobQueue(jobQueueItem);

			JobRepository.AssignJobToWorkerNode(HttpSender);
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
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};

			IHttpActionResult response = ManagerController.AddItemToJobQueue(jobQueueItem);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetUniqueJobIdWhilePersistingJob()
		{
			var response1 = ManagerController.AddItemToJobQueue(new JobQueueItem
			{
				Name = "Name Test 1",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			});

			var response2 = ManagerController.AddItemToJobQueue(new JobQueueItem
			{
				Name = "Name Test 2",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			});

			var distinctJobIdCount =
				JobRepository.GetAllItemsInJobQueue().Select(item => item.JobId).Distinct().Count();

			distinctJobIdCount.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotBeAbleToPersist_WhenInvalidJobQueueItem()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "",
				CreatedBy = "Created By Test"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsTrue(response is BadRequestErrorMessageResult);

			JobRepository.GetAllItemsInJobQueue().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveAQueuedJob()
		{
			NodeRepository.AddWorkerNode(new WorkerNode {Url = _nodeUri1});

			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test",
				JobId = Guid.NewGuid()
			};

			JobRepository.AddItemToJobQueue(jobQueueItem);

			JobRepository.AssignJobToWorkerNode(HttpSender);

			ManagerController.CancelJobByJobId(jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnBadRequestIfCancelThisJobGetsAnInvalidGuid()
		{
			var response = ManagerController.CancelJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfHeartbeatGetsAnInvalidUri()
		{
			var response = ManagerController.WorkerNodeRegisterHeartbeat(null);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsANull()
		{
			var response = ManagerController.JobFailed(null);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsInvalidJobFailedMode()
		{
			var jobFailed = new JobFailed();

			var response = ManagerController.JobFailed(jobFailed);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryDetailsGetsAnInvalidGuid()
		{
			var response = ManagerController.GetJobDetailsByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryGetsAnInvalidGuid()
		{
			var response = ManagerController.GetJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobQueueItemHasInvalidCreatedByValue()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = string.Empty
			};

			var
				response = ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = string.Empty,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = null,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response = ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = string.Empty,
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = null,
				Type = "Type",
				CreatedBy = "UserName"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsEmptyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = string.Empty,
				CreatedBy = "UserName"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = null,
				CreatedBy = "UserName"
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelUserNameIsNull()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = null
			};

			var response =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}


		[Test]
		public void ShouldReturnBadRequestIJobDoneGetsAnInvalidUri()
		{
			var response = ManagerController.JobSucceed(Guid.Empty);

			Assert.IsInstanceOf(typeof (BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnConflictIfNodeIsBusy()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "ShouldReturnConflictIfNodeIsBusy",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};

			ThisNodeIsBusy(_nodeUri1.ToString());

			ManagerController.WorkerNodeRegisterHeartbeat(_nodeUri1);

			ManagerController.AddItemToJobQueue(jobQueueItem);

			HttpSender.CallToWorkerNodes.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnIdOfPersistedJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "ShouldReturnIdOfPersistedJob",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created by Test"
			};

			IHttpActionResult actionResult =
				ManagerController.AddItemToJobQueue(jobQueueItem);

			var okNegotiatedContentResult = actionResult as OkNegotiatedContentResult<Guid>;
			var jobId = okNegotiatedContentResult.Content;

			jobId.Satisfy(guid => guid != Guid.Empty);
		}

		[Test]
		public void ShouldSendOkWhenJobDoneSignalReceived()
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = Guid.NewGuid(),
				Name = "ShouldSendOkWhenJobDoneSignalReceived",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created by Test"
			};

			JobRepository.AddItemToJobQueue(jobQueueItem);

			var result =
				ManagerController.JobSucceed(jobQueueItem.JobId);

			result.Should().Not.Be.Null();
		}
	}
}