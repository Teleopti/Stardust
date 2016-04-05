using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
	public class JobManagerTests : DatabaseTest
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
			var jobId = Guid.NewGuid();

			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "Serialized",
				Type = "Type"
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			});

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobManager.CancelJobByJobId(jobId);
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);
			job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Canceling");
		}


		[Test]
		public void ShouldContainJobManager()
		{
			JobManager.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldJumpOutIfNoJob()
		{
			JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldJumpOutIfNoNode()
		{
			var id = Guid.NewGuid();
			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = id,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Should().Be.Empty();
		}

		[Test]
		public void ShouldJustDeleteJobIfNotStarted()
		{
			var jobId = Guid.NewGuid();

			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				JobProgress = "Waiting",
				Serialized = "Serialized",
				AssignedNode = "",
				Status = "Added",
				Type = "Type"
			});

			var job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Not.Be.Null();
			JobManager.CancelJobByJobId(jobId);
			FakeHttpSender.CalledNodes.Should().Be.Empty();
			job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Be.Null();
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceInInit()
		{
			NodeManager.AddIfNeeded(_nodeUri1);
			NodeManager.AddIfNeeded(_nodeUri1);
			WorkerNodeRepository.GetAllWorkerNodes()
				.Count.Should()
				.Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveJobOnBadRequest()
		{
			var jobId = Guid.NewGuid();
			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(), Url = _nodeUri1
			});

			FakeHttpSender.Responses = new List<HttpResponseMessage>
			{
				new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					ReasonPhrase = "Reason comes here."
				}
			};

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Be.Null();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobRepository.GetJobHistoryByJobId(jobId).Result.Should().Contain("Removed");
		}

		[Test]
		public void ShouldResetAssignedNodeOnInitIfItIsAssigned()
		{
			var jobId = Guid.NewGuid();
			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.AddWorkerNode(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});
			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());

			NodeManager.FreeJobIfAssingedToNode(_nodeUri1);
			job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.Null();
		}

		[Test]
		public void ShouldSendJobToNode()
		{
			var jobId = Guid.NewGuid();
			var nodeId = Guid.NewGuid();

			JobRepository.AddJobDefinition(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				JobProgress = "Waiting",
				Serialized = "Serialized",
				AssignedNode = "",
				Status = "Added",
				Type = "Type"
			});

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = nodeId, Url = _nodeUri1
			});

			JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);

			var job =
				JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));

			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldSendJobToNodeLoadTest()
		{
			var numberOfJobs = 100;

			var tasks = new List<Task>();

			for (var i = 0; i < numberOfJobs; i++)
			{
				tasks.Add(new Task(() =>
				{
					JobRepository.AddJobDefinition(new JobDefinition
					{
						Id = Guid.NewGuid(),
						Name = "For test",
						UserName = "JobManagerTests",
						JobProgress = "Waiting",
						Serialized = "Serialized",
						AssignedNode = "",
						Status = "Added",
						Type = "Type"
					});
				}));
			}

			Parallel.ForEach(tasks, task => { task.Start(); });


			Task.WaitAll(tasks.ToArray());

			var all = JobRepository.GetAllJobDefinitions().Count;

			Assert.IsTrue(numberOfJobs == all, "Should be equal");
		}

		[Test]
		public void ShouldSetEndResultInHistoryAndRemoveTheJobWhenJobIsDone()
		{
			var jobId = Guid.NewGuid();

			var jobDefinition = new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			};

			JobRepository.AddJobDefinition(jobDefinition);

			WorkerNodeRepository.AddWorkerNode(new WorkerNode
			{
				Id = Guid.NewGuid(),
				Url = _nodeUri1
			});

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.GetAllJobDefinitions().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobManager.SetEndResultOnJobAndRemoveIt(jobId, "Success");
			JobManager.GetAllJobHistories().Should().Not.Be.Empty();
			JobManager.GetJobHistoryDetailsByJobId(jobId).Should().Not.Be.Empty();
		}
	}
}