using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
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
	public class ManagerOperationsTest 
	{
		public ManagerController Target;
		public IJobRepository JobRepository;
		public IWorkerNodeRepository NodeRepository;
		public INodeManager NodeManager;
		public IManagerConfiguration ManagerConfiguration;
		public FakeHttpSender HttpSender;
		private DatabaseHelper databaseHelper;
		private readonly Uri _nodeUri1 = new Uri("http://localhost:9050/");
		private readonly Uri _nodeUri2 = new Uri("http://localhost:9051/");

		private void ThisNodeIsBusy(string url)
		{
			HttpSender.BusyNodesUrl.Add(url);
		}

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			databaseHelper = new DatabaseHelper();
			databaseHelper.Create();
		}

		[SetUp]
		public void Setup()
		{
			databaseHelper.TryClearDatabase();
		}

		[Test]
		public void ResetJobsOnFalseClaimOnHeartBeatIfItsFree()
		{
			var jobId = Guid.NewGuid();
			var userName = "ManagerTests";
			var job = new JobDefinition
			{
				Id = jobId,
				Name = "job",
				UserName = userName,
				Serialized = "Fake Serialized",
				Type = "Fake Type"
			};
			JobRepository.Add(job);
			JobRepository.CheckAndAssignNextJob(new List<WorkerNode> {new WorkerNode {Url = _nodeUri1}},
			                                    HttpSender);
			Target.Heartbeat(_nodeUri1.ToString());
			HttpSender.CalledNodes.First()
				.Key.Should()
				.Contain(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldAddANodeOnInit()
		{
			Target.NodeInitialized(_nodeUri1);
			NodeRepository.LoadAll()
				.First()
				.Url.Should()
				.Be.EqualTo(_nodeUri1.ToString());
		}


		[Test]
		public void ShouldBeAbleToAcknowledgeWhenJobIsReceived()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToAcknowledgeWhenJobIsReceived",
				Serialized = "ngt",
				Type = "bra",
				UserName = "ManagerTests"
			};
			var result = Target.DoThisJob(job);
			result.Should()
				.Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToCancelJobOnNode()
		{
			Target.Heartbeat(_nodeUri1.ToString());
			Target.Heartbeat(_nodeUri2.ToString());

			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition {Id = jobId, Serialized = "", Name = "", Type = "", UserName = "ManagerTests"});
			JobRepository.CheckAndAssignNextJob(
				new List<WorkerNode> {new WorkerNode {Url = _nodeUri1}, new WorkerNode {Url = _nodeUri2}},
				HttpSender);
			HttpSender.CalledNodes.Clear();
			Target.CancelThisJob(jobId);
			HttpSender.CalledNodes.Count()
				.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToPersistManyJobs()
		{
			var stopwatch = new Stopwatch();

			var jobRequestModels = new List<JobRequestModel>();

			for (var i = 0; i < 500; i++)
			{
				jobRequestModels.Add(new JobRequestModel
				{
					Name = "Name data " + i,
					Serialized = "ngtbara",
					Type = "typngtannat",
					UserName = "ManagerTests"
				});
			}

			var tasks = new List<Task>();

			foreach (var jobRequestModel in jobRequestModels)
			{
				var model = jobRequestModel;

				tasks.Add(new Task(() => { Target.DoThisJob(model); }));
			}

			stopwatch.Start();

			Parallel.ForEach(tasks,
			                 task => { task.Start(); });

			Task.WaitAll(tasks.ToArray());

			stopwatch.Stop();

			var elapsed = stopwatch.Elapsed;

			var sec = elapsed.Seconds;

			Assert.IsTrue(true);
		}


		[Test]
		public void ShouldBeAbleToPersistNewJob()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToPersistNewJob",
				Serialized = "ngtbara",
				Type = "typngtannat",
				UserName = "ManagerTests"
			};
			Target.DoThisJob(job);
			JobRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(1);
		}


		[Test]
		public void ShouldGetUniqueJobIdWhilePersistingJob()
		{
			Target.DoThisJob(new JobRequestModel
			{
				Name = "ShouldGetUniqueJobIdWhilePersistingJob",
				Serialized = "ngt",
				Type = "bra",
				UserName = "ManagerTests"
			});
			Target.DoThisJob(new JobRequestModel
			{
				Name = "ShouldGetUniqueJobIdWhilePersistingJob",
				Serialized = "ngt",
				Type = "bra",
				UserName = "ManagerTests"
			});

			JobRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceInInit()
		{
			Target.NodeInitialized(_nodeUri1);
			Target.NodeInitialized(_nodeUri1);
			NodeRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotRemoveARunningJobFromRepo()
		{
			var jobId = Guid.NewGuid();
			var job = new JobDefinition {Name = " ", Serialized = " ", Type = " ", UserName = "ManagerTests", Id = jobId};
			JobRepository.Add(job);
			JobRepository.CheckAndAssignNextJob(new List<WorkerNode> {new WorkerNode {Url = _nodeUri1}},
			                                    HttpSender);
			ThisNodeIsBusy(_nodeUri1.ToString());
			Target.CancelThisJob(jobId);
			JobRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveAQueuedJob()
		{
			var jobId = Guid.NewGuid();
			var job = new JobDefinition {Name = "", Serialized = "", Type = "", UserName = "ManagerTests", Id = jobId};
			JobRepository.Add(job);
			Target.CancelThisJob(jobId);
			JobRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveTheJobWhenItsFinished()
		{
			var jobId = Guid.NewGuid();
			var job = new JobDefinition
			{
				Id = jobId,
				AssignedNode = _nodeUri1.ToString(),
				Name = "job",
				Serialized = "",
				Type = "",
				UserName = "ShouldRemoveTheJobWhenItsFinished"
			};
			JobRepository.Add(job);
			Target.JobDone(job.Id);
			JobRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnConflictIfNodeIsBusy()
		{
			var job = new JobRequestModel
			{
				Name = "ShouldBeAbleToSendNewJobToAvailableNode",
				Serialized = "ngt",
				Type = "bra",
				UserName = "ManagerTests"
			};
			ThisNodeIsBusy(_nodeUri1.ToString());

			Target.Heartbeat(_nodeUri1.ToString());

			Target.DoThisJob(job);

			HttpSender.CalledNodes.Count.Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnIdOfPersistedJob()
		{
			var newJobId =
				((OkNegotiatedContentResult<Guid>)
					Target.DoThisJob(new JobRequestModel
					{
						Name = "ShouldReturnIdOfPersistedJob",
						Serialized = "ngt",
						Type = "bra",
						UserName = "ManagerTests"
					})).Content;
			newJobId.Should()
				.Not.Be.Null();
		}

		[Test]
		public void ShouldReturnJobHistoryFromJobId()
		{
			var job = new JobRequestModel {Name = "Name", Serialized = "Ser", Type = "Type", UserName = "ManagerTests"};

			var doJobResult = Target.DoThisJob(job);

			var okNegotiatedDoJobResult = doJobResult as OkNegotiatedContentResult<Guid>;
			var jobId = okNegotiatedDoJobResult.Content;

			var getResult = Target.JobHistory(jobId);

			var okNegotiatedGetResult = getResult as OkNegotiatedContentResult<JobHistory>;
			var jobHistory = okNegotiatedGetResult.Content;
			Assert.IsNotNull(jobHistory);
		}

		[Test]
		public void ShouldSendOkWhenJobDoneSignalReceived()
		{
			var jobId = Guid.NewGuid();
			var job = new JobDefinition
			{
				Id = jobId,
				AssignedNode = _nodeUri1.ToString(),
				Name = "job",
				Serialized = "",
				Type = "",
				UserName = "ShouldSendOkWhenJobDoneSignalReceived"
			};
			JobRepository.Add(job);
			var result = Target.JobDone(job.Id);
			result.Should()
				.Not.Be.Null();
		}
	}
}