using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public ManagerController Target;
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

		[SetUp]
		public void Setup()
		{
			_databaseHelper.TryClearDatabase();
		}

		[Test]
		public void ResetJobsOnFalseClaimOnHeartBeatIfItsFree()
		{
			var jobId = Guid.NewGuid();
			var userName = "ManagerTests";
			var job = new JobQueueItem
			{
				JobId = jobId,
				Name = "job",
				CreatedBy = userName,
				Serialized = "Fake Serialized",
				Type = "Fake Type"
			};

			JobRepository.AddItemToJobQueue(job);
			NodeRepository.AddWorkerNode(new WorkerNode { Url = _nodeUri1 });

			JobRepository.TryAssignJobToWorkerNode(HttpSender);

			Target.RegisterHeartbeat(_nodeUri1);
			HttpSender.CalledNodes.First()
				.Key.Should()
				.Contain(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldBeAbleToAcknowledgeWhenJobIsReceived()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToAcknowledgeWhenJobIsReceived",
				Serialized = "ngt",
				Type = "bra",
				CreatedBy = "ManagerTests"
			};
			var result = Target.AddItemToJobQueue(job);
			result.Should()
				.Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToCancelJobOnNode()
		{
			NodeRepository.AddWorkerNode(new WorkerNode { Url = _nodeUri1 });
			NodeRepository.AddWorkerNode(new WorkerNode { Url = _nodeUri2 });
			Target.RegisterHeartbeat(_nodeUri1);
			Target.RegisterHeartbeat(_nodeUri2);

			var jobId = Guid.NewGuid();
			JobRepository.AddItemToJobQueue(new JobQueueItem {JobId = jobId, Serialized = "", Name = "", Type = "", CreatedBy = "ManagerTests"});
			JobRepository.TryAssignJobToWorkerNode(HttpSender);
			HttpSender.CalledNodes.Clear();
			Target.CancelJobByJobId(jobId);
			HttpSender.CalledNodes.Count()
				.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToPersistManyJobs()
		{
			var jobRequestModels = new List<JobRequestModel>();

			for (var i = 0; i < 50; i++)
			{
				var jobRequestModel = new JobRequestModel
				{
					Name = "Name data " + i,
					Serialized = "Serialized",
					Type = "Type",
					CreatedBy = "User name"
				};

				jobRequestModels.Add(jobRequestModel);
			}

			var tasks = new List<Task>();

			foreach (var jobRequestModel in jobRequestModels)
			{
				var model = jobRequestModel;

				tasks.Add(new Task(() =>
				{
					Target.AddItemToJobQueue(model);
				}));
			}

			Parallel.ForEach(tasks,
			                 task => { task.Start(); });

			Task.WaitAll(tasks.ToArray());

			var faultedExists = tasks.Exists(task => task.IsFaulted);

			Assert.IsFalse(faultedExists);
		}


		[Test]
		public void ShouldBeAbleToPersistBadRequestResonsToHistoryDetail()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToPersistNewJob",
				Serialized = "ngtbara",
				Type = "typngtannat",
				CreatedBy = "ManagerTests"
			};

			Target.AddItemToJobQueue(job);

			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToPersistNewJob()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToPersistNewJob",
				Serialized = "ngtbara",
				Type = "typngtannat",
				CreatedBy = "ManagerTests"
			};

			var response=Target.AddItemToJobQueue(job);

			if (response is BadRequestErrorMessageResult)
			{
				Assert.Fail("Invalid job request model.");
			}
			else
			{
				JobRepository.GetAllItemsInJobQueue()
					.Count.Should()
					.Be.EqualTo(1);
			}
		}


		[Test]
		public void ShouldGetUniqueJobIdWhilePersistingJob()
		{
			var response=Target.AddItemToJobQueue(new JobRequestModel
			{
				Name = "ShouldGetUniqueJobIdWhilePersistingJob",
				Serialized = "ngt",
				Type = "bra",
				CreatedBy = "ManagerTests"
			});

			if (response is BadRequestErrorMessageResult)
			{
				Assert.Fail("Invalid job request model 1.");	
			}

			response = Target.AddItemToJobQueue(new JobRequestModel
			{
				Name = "ShouldGetUniqueJobIdWhilePersistingJob",
				Serialized = "ngt",
				Type = "bra",
				CreatedBy = "ManagerTests"
			});

			if (response is BadRequestErrorMessageResult)
			{
				Assert.Fail("Invalid job request model 2.");
			}
			else
			{
				JobRepository.GetAllItemsInJobQueue()
					.Count.Should()
					.Be.EqualTo(2);

			}
		}

		[Test]
		public void ShouldNotRemoveARunningJobFromRepo()
		{
			var jobId = Guid.NewGuid();

			var job = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "ManagerTests",
				JobId = jobId
			};

			JobRepository.AddItemToJobQueue(job);
			NodeRepository.AddWorkerNode(new WorkerNode
			{
				Url = _nodeUri1
			});
			JobRepository.TryAssignJobToWorkerNode(HttpSender);
			ThisNodeIsBusy(_nodeUri1.ToString());
			Target.CancelJobByJobId(jobId);
			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveAQueuedJob()
		{
			var jobId = Guid.NewGuid();

			var job = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "ManagerTests",
				JobId = jobId
			};

			JobRepository.AddItemToJobQueue(job);
			Target.CancelJobByJobId(jobId);
			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsNull()
		{
			var job = new JobRequestModel
			{
				Name = null,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			IHttpActionResult response=Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult),response);

		}

		[Test]
		public void ShouldReturnBadRequestIfHeartbeatGetsAnInvalidUri()
		{
			var response = Target.RegisterHeartbeat(null);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsInvalidJobFailedMode()
		{
			JobFailedModel jobFailedModel=new JobFailedModel();

			var response = Target.JobFailed(jobFailedModel);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobFailedGetsANull()
		{
			var response = Target.JobFailed(null);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}


		[Test]
		public void ShouldReturnBadRequestIJobDoneGetsAnInvalidUri()
		{
			var response = Target.JobDone(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryDetailsGetsAnInvalidGuid()
		{
			var response = Target.GetJobDetailsByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobHistoryGetsAnInvalidGuid()
		{
			var response = Target.GetJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfCancelThisJobGetsAnInvalidGuid()
		{
			var response=Target.CancelJobByJobId(Guid.Empty);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);
		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelNameIsEmptyString()
		{
			var job = new JobRequestModel
			{
				Name = string.Empty,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsNull()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = null,
				Type = "Type",
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelSerializedIsEmptyString()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = string.Empty,
				Type = "Type",
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsNull()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = null,
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelTypeIsEmptyString()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = string.Empty,
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelUserNameIsNull()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = null
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelUserNameIsEmtpyString()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = string.Empty
			};

			IHttpActionResult response = Target.AddItemToJobQueue(job);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnConflictIfNodeIsBusy()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToSendNewJobToAvailableNode",
				Serialized = "ngt",
				Type = "bra",
				CreatedBy = "ManagerTests"
			};
			ThisNodeIsBusy(_nodeUri1.ToString());

			Target.RegisterHeartbeat(_nodeUri1);

			Target.AddItemToJobQueue(job);

			HttpSender.CalledNodes.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnIdOfPersistedJob()
		{
			var newJobId =
				((OkNegotiatedContentResult<Guid>)
					Target.AddItemToJobQueue(new JobRequestModel
					{
						Name = "ShouldReturnIdOfPersistedJob",
						Serialized = "ngt",
						Type = "bra",
						CreatedBy = "ManagerTests"
					})).Content;
			newJobId.Should()
				.Not.Be.Null();
		}

		[Test]
		public void ShouldReturnJobHistoryFromJobId()
		{
			var job = new JobRequestModel
			{
				Name = "Name",
				Serialized = "Ser",
				Type = "Type",
				CreatedBy = "ManagerTests"
			};

			var doJobResult = Target.AddItemToJobQueue(job);

			var okNegotiatedDoJobResult = doJobResult as OkNegotiatedContentResult<Guid>;
			var jobId = okNegotiatedDoJobResult.Content;

			var getResult = Target.GetJobByJobId(jobId);

			var okNegotiatedGetResult = getResult as OkNegotiatedContentResult<Job>;
			var jobHistory = okNegotiatedGetResult.Content;
			Assert.IsNotNull(jobHistory);
		}

		[Test]
		public void ShouldSendOkWhenJobDoneSignalReceived()
		{
			var jobId = Guid.NewGuid();
			var job = new JobQueueItem
			{
				JobId = jobId,
				Name = "job",
				Serialized = "",
				Type = "",
				CreatedBy = "ShouldSendOkWhenJobDoneSignalReceived"
			};
			JobRepository.AddItemToJobQueue(job);
			var result = Target.JobDone(job.JobId);
			result.Should()
				.Not.Be.Null();
		}
	}
}