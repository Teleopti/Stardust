using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ManagerTest.Database;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[TestFixture, JobTests]
	public class JobManagerTests : DatabaseTest
	{
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

		[TearDown]
		public void TearDown()
		{
			JobManager.Dispose();
		}

		[Test]
		public void ShouldBeAbleToCancelJobIfStarted()
		{
			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobManager.CancelThisJob(jobId);
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);
			job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Canceling");
		}

		[Test]
		public void ShouldCheckIfNodeIsUpAndRunningBeforeTryAssignJob()
		{
			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri2});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri3});

			var builderHelper1 = new NodeUriBuilderHelper(_nodeUri1);
			var builderHelper2 = new NodeUriBuilderHelper(_nodeUri2);
			var builderHelper3 = new NodeUriBuilderHelper(_nodeUri3);

			var urijob1 = builderHelper1.GetJobTemplateUri();
			var urijob2 = builderHelper2.GetJobTemplateUri();
			var urijob3 = builderHelper3.GetJobTemplateUri();

			FakeHttpSender.BusyNodesUrl.Add(_nodeUri1.ToString());
			FakeHttpSender.BusyNodesUrl.Add(_nodeUri3.ToString());
			JobManager.CheckAndAssignNextJob();

			FakeHttpSender.CalledNodes[urijob2.ToString()].Should().Not.Be.Null();
			FakeHttpSender.CalledNodes.Keys.Should().Not.Contain(urijob1.ToString());
			FakeHttpSender.CalledNodes.Keys.Should().Not.Contain(urijob3.ToString());
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
			JobRepository.Add(new JobDefinition
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
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				JobProgress = "Waiting",
				Serialized = "",
				AssignedNode = "",
				Status = "Added",
				Type = ""
			});

			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Not.Be.Null();
			JobManager.CancelThisJob(jobId);
			FakeHttpSender.CalledNodes.Should().Be.Empty();
			job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveJobOnBadRequest()
		{
			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});
			FakeHttpSender.Responses = new List<HttpResponseMessage> {new HttpResponseMessage(HttpStatusCode.BadRequest)};

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Be.Null();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobRepository.History(jobId).Result.Should().Contain("Removed");
		}

		[Test]
		public void ShouldResetAssignedNodeOnInitIfItIsAssigned()
		{
			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});
			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());

			NodeManager.FreeJobIfAssingedToNode(_nodeUri1);
			job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.Null();
		}

		[Test]
		public void ShouldSendJobToNode()
		{
			var jobId = Guid.NewGuid();
			var nodeId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				JobProgress = "Waiting",
				Serialized = "",
				AssignedNode = "",
				Status = "Added",
				Type = ""
			});

			WorkerNodeRepository.Add(new WorkerNode {Id = nodeId, Url = _nodeUri1});
			JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);

			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldSetEndResultInHistoryAndRemoveTheJobWhenJobIsDone()
		{
			var jobId = Guid.NewGuid();
			JobRepository.Add(new JobDefinition
			{
				Id = jobId,
				Name = "For test",
				UserName = "JobManagerTests",
				Serialized = "",
				Type = ""
			});
			WorkerNodeRepository.Add(new WorkerNode {Id = Guid.NewGuid(), Url = _nodeUri1});

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(1);
			JobManager.SetEndResultOnJobAndRemoveIt(jobId, "Success");
			JobManager.GetJobHistoryList().Should().Not.Be.Empty();
			JobManager.JobHistoryDetails(jobId).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAddANodeOnInit()
		{
			NodeManager.AddIfNeeded(_nodeUri1);
			WorkerNodeRepository.LoadAll()
				.First()
				.Url.Should()
				.Be.EqualTo(_nodeUri1.ToString());
		}

		[Test]
		public void ShouldNotAddSameNodeTwiceInInit()
		{
			NodeManager.AddIfNeeded(_nodeUri1);
			NodeManager.AddIfNeeded(_nodeUri1);
			WorkerNodeRepository.LoadAll()
				.Count.Should()
				.Be.EqualTo(1);
		}
	}
}