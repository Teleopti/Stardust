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

			Target.RegisterHeartbeat(_nodeUri1);

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

			var result = Target.AddItemToJobQueue(job);

			result.Should()
				.Not.Be.Null();
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

			Target.RegisterHeartbeat(_nodeUri1);
			Target.RegisterHeartbeat(_nodeUri2);

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
			Target.CancelJobByJobId(jobQueueItem.JobId);
			HttpSender.CallToWorkerNodes.Count()
				.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToPersistManyJobs()
		{
			var jobQueueItems = new List<JobQueueItem>();

			for (var i = 0; i < 50; i++)
			{
				var jobQueueItem = new JobQueueItem
				{
					Name = "Name Test " + i,
					Serialized = "Serialized Test",
					Type = "Type Test",
					CreatedBy = "Created By Test"
				};

				jobQueueItems.Add(jobQueueItem);
			}

			var tasks = new List<Task>();

			foreach (var jobQueueItem in jobQueueItems)
			{
				var model = jobQueueItem;

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
		public void ShouldBeAbleToPersistBadRequestResonsToJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};

			Target.AddItemToJobQueue(jobQueueItem);

			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToPersistNewJob()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			};

			var response=Target.AddItemToJobQueue(jobQueueItem);

			if (response is BadRequestErrorMessageResult)
			{
				Assert.Fail("Invalid job queue item.");
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
			var response=Target.AddItemToJobQueue(new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
			});

			if (response is BadRequestErrorMessageResult)
			{
				Assert.Fail("Invalid job request model 1.");	
			}

			response = Target.AddItemToJobQueue(new JobQueueItem
			{
				Name = "Name Test",
				Serialized = "Serialized Test",
				Type = "Type Test",
				CreatedBy = "Created By Test"
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
		public void ShouldRemoveAQueuedJob()
		{
			NodeRepository.AddWorkerNode(new WorkerNode { Url = _nodeUri1 });

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

			Target.CancelJobByJobId(jobQueueItem.JobId);

			JobRepository.GetAllItemsInJobQueue()
				.Count.Should()
				.Be.EqualTo(0);
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

			IHttpActionResult response=Target.AddItemToJobQueue(jobQueueItem);

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
			JobFailed jobFailed=new JobFailed();

			var response = Target.JobFailed(jobFailed);

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
			var jobQueueItem = new JobQueueItem
			{
				Name = string.Empty,
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = "UserName"
			};

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

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

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

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

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

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

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

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

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

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

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnBadRequestIfJobRequestModelUserNameIsEmtpyString()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "Name",
				Serialized = "Serialized",
				Type = "Type",
				CreatedBy = string.Empty
			};

			IHttpActionResult response = Target.AddItemToJobQueue(jobQueueItem);

			Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), response);

		}

		[Test]
		public void ShouldReturnConflictIfNodeIsBusy()
		{
			var jobQueueItem = new JobQueueItem
			{
				Name = "ShouldBeAbleToSendNewJobToAvailableNode",
				Serialized = "ngt",
				Type = "bra",
				CreatedBy = "ManagerTests"
			};
			ThisNodeIsBusy(_nodeUri1.ToString());

			Target.RegisterHeartbeat(_nodeUri1);

			Target.AddItemToJobQueue(jobQueueItem);

			HttpSender.CallToWorkerNodes.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnIdOfPersistedJob()
		{
			var newJobId =
				((OkNegotiatedContentResult<Guid>)
					Target.AddItemToJobQueue(new JobQueueItem
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