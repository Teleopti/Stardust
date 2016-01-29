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
using Stardust.Manager.Constants;
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

        private FakeHttpSender FakeHttpSender
		{
			get { return (FakeHttpSender) HttpSender; }
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
         JobRepository.Add(new JobDefinition {Id = id,Name = "For test", UserName = "JobManagerTests", Serialized = "", Type = ""});
			JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Should().Be.Empty();
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
			WorkerNodeRepository.Add(new WorkerNode {Id = nodeId , Url = _nodeUri1.ToString() });
         JobManager.CheckAndAssignNextJob();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);

			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());
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
		public void ShouldCallNodeToCancelIfStarted()
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
			WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = _nodeUri1.ToString() });

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);
			JobManager.CancelThisJob(jobId);
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(3);
			job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			// how will we handle status?
			job.Status.Should().Be.EqualTo("Canceling");
		}

		[Test]
		public void ShouldSetEndResultInHistoryAndRemoveTheJob()
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
			WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = _nodeUri1.ToString() });

			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Status.Should().Be.EqualTo("Started");
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);
			JobManager.SetEndResultOnJobAndRemoveIt(jobId, "Success");
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
			WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = _nodeUri1.ToString() });
			FakeHttpSender.Responses = new List<HttpResponseMessage> { new HttpResponseMessage(HttpStatusCode.OK) , new HttpResponseMessage(HttpStatusCode.BadRequest) };
				
         JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.Should().Be.Null();
			FakeHttpSender.CalledNodes.Count.Should().Be.EqualTo(2);
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
			WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = _nodeUri1.ToString() });
			JobManager.CheckAndAssignNextJob();
			var job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			job.AssignedNode.Should().Be.EqualTo(_nodeUri1.ToString());

			NodeManager.FreeJobIfAssingedToNode(_nodeUri1.ToString());
			job = JobRepository.LoadAll().FirstOrDefault(j => j.Id.Equals(jobId));
			// how will we handle status?
			job.AssignedNode.Should().Be.Null();
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
			WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = "http://Url1/" });
            WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = "http://Url2/" });
            WorkerNodeRepository.Add(new WorkerNode { Id = Guid.NewGuid(), Url = "http://Url3/" });

            FakeHttpSender.BusyNodesUrl.Add("Url1");
			FakeHttpSender.BusyNodesUrl.Add("Url3");
			JobManager.CheckAndAssignNextJob();

            FakeHttpSender.CalledNodes["http://Url2/" + NodeRouteConstants.Job].Should().Not.Be.Null();
			FakeHttpSender.CalledNodes.Keys.Should().Not.Contain("http://Url1/" + NodeRouteConstants.Job);
			FakeHttpSender.CalledNodes.Keys.Should().Not.Contain("http://Url3/" + NodeRouteConstants.Job);
		}
	}
}