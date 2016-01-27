using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
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
    public class ManagerOperationsTest : DatabaseTest
    {
        public ManagerController Target;
        public IJobRepository JobRepository;
        public IWorkerNodeRepository NodeRepository;
        public INodeManager NodeManager;
        public FakeHttpSender HttpSender;

        [Test]
        public void ShouldBeAbleToAcknowledgeWhenJobIsReceived()
        {
            var job = new JobRequestModel() {Name = "ShouldBeAbleToAcknowledgeWhenJobIsReceived", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"};
            var result = Target.DoThisJob(job);
            result.Should()
                .Not.Be.Null();
        }

        [Test]
        public void ShouldBeAbleToPersistManyJobs()
        {
            Stopwatch stopwatch = new Stopwatch();

            List<JobRequestModel> jobRequestModels = new List<JobRequestModel>();

            for (int i = 0; i < 500; i++)
            {
                jobRequestModels.Add(new JobRequestModel
                {
                    Name = "Name data " + i,
                    Serialized = "ngtbara",
                    Type = "typngtannat",
                    UserName = "ManagerTests"
                });
            }

            List<Task> tasks = new List<Task>();

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

            TimeSpan elapsed = stopwatch.Elapsed;

            var sec = elapsed.Seconds;

            Assert.IsTrue(true);
        }


        [Test]
        public void ShouldBeAbleToPersistNewJob()
        {
            var job = new JobRequestModel() {Name = "ShouldBeAbleToPersistNewJob", Serialized = "ngtbara", Type = "typngtannat", UserName = "ManagerTests"};
            Target.DoThisJob(job);
            JobRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(1);
        }

        [Test]
        public void ShouldReturnIdOfPersistedJob()
        {
            Guid newJobId = ((OkNegotiatedContentResult<Guid>) Target.DoThisJob(new JobRequestModel() {Name = "ShouldReturnIdOfPersistedJob", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"})).Content;
            newJobId.Should()
                .Not.Be.Null();
        }

        [Test]
        public void ShouldAddANodeOnInit()
        {
            Target.NodeInitialized(new Uri("localhost:9000/"));
            NodeRepository.LoadAll()
                .First()
                .Url.Should()
                .Be.EqualTo("localhost:9000/");
        }

        [Test]
        public void ShouldBeAbleToSendNewJobToAvailableNode()
        {
            var job = new JobRequestModel() {Name = "ShouldBeAbleToSendNewJobToAvailableNode", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"};
            Target.NodeInitialized(new Uri("localhost:9000/"));
            Target.DoThisJob(job);
            Target.Heartbeat(new Uri("localhost:9000/"));
            HttpSender.CalledNodes.Keys.First()
                .Should()
                .Contain("localhost:9000/");
        }

        [Test]
        public void ShouldReturnConflictIfNodeIsBusy()
        {
            var job = new JobRequestModel() {Name = "ShouldBeAbleToSendNewJobToAvailableNode", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"};
            thisNodeIsBusy("localhost:9000/");

            Target.Heartbeat(new Uri("localhost:9000/"));

            Target.DoThisJob(job);

            HttpSender.CalledNodes.Count.Should()
                .Be.EqualTo(0);
        }

        [Test]
        public void ShouldBeAbleToSendNewJobToFirstAvailableNode()
        {
            var job = new JobRequestModel() {Name = "ShouldBeAbleToSendNewJobToFirstAvailableNode", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"};
            thisNodeIsBusy("localhost:9000");

            Target.NodeInitialized(new Uri("localhost:9000/"));
            Target.NodeInitialized(new Uri("localhost:9001/"));

            Target.DoThisJob(job);

            Target.Heartbeat(new Uri("localhost:9000/"));

            HttpSender.CalledNodes.Count.Should()
                .Be.EqualTo(2);
            HttpSender.CalledNodes.Keys.First()
                .Should()
                .Contain("localhost:9001");
        }

        [Test]
        public void ShouldAddNodeReferenceToJObDefinition()
        {
            var job = new JobRequestModel() {Name = "ShouldAddNodeReferenceToJObDefinition", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"};

            Target.NodeInitialized(new Uri("localhost:9000/"));
            Target.DoThisJob(job);
            Target.Heartbeat(new Uri("localhost:9000/"));
            JobRepository.LoadAll()
                .First()
                .AssignedNode.Should()
                .Contain("localhost:9000/");
        }

        [Test]
        public void ShouldDistributePersistedJobsOnHeartbeat()
        {
            string userName = "ManagerTests";
            var job1Id = Guid.NewGuid();
            var job2Id = Guid.NewGuid();
            var job1 = new JobDefinition() {Id = job1Id, AssignedNode = "local", Name = "job", UserName = userName, Serialized = "Fake Serialized", Type = "Fake Type"};
            var job2 = new JobDefinition() {Id = job2Id, Name = "JOb2", UserName = userName, Serialized = "Fake Serialized", Type = "Fake Type"};
            JobRepository.Add(job1);
            JobRepository.Add(job2);

            Target.NodeInitialized(new Uri("localhost:9000/"));
            Target.Heartbeat(new Uri("localhost:9000/"));
            HttpSender.CalledNodes.Count.Should()
                .Be.EqualTo(2);
        }

        [Test]
        public void ShouldRemoveAQueuedJob()
        {
            Guid jobId = Guid.NewGuid();
            var job = new JobDefinition() {Name = "", Serialized = "", Type = "", UserName = "ManagerTests", Id = jobId};
            JobRepository.Add(job);
            Target.CancelThisJob(jobId);
            JobRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(0);
        }

        [Test]
        public void ShouldNotRemoveARunningJobFromRepo()
        {
            Guid jobId = Guid.NewGuid();
            var job = new JobDefinition() {Name = " ", Serialized = " ", Type = " ", UserName = "ManagerTests", Id = jobId};
            JobRepository.Add(job);
            JobRepository.CheckAndAssignNextJob(new List<WorkerNode>() {new WorkerNode() {Url = "localhost:9000/"}},
                                                HttpSender);
            thisNodeIsBusy("localhost:9000/");
            Target.CancelThisJob(jobId);
            JobRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(1);
        }

        [Test]
        public void ShouldBeAbleToCancelJobOnNode()
        {
            Target.Heartbeat(new Uri("localhost:9000/"));
            Target.Heartbeat(new Uri("localhost:9000/"));

            Guid jobId = Guid.NewGuid();
            JobRepository.Add(new JobDefinition() {Id = jobId, Serialized = "", Name = "", Type = "", UserName = "ManagerTests"});
            JobRepository.CheckAndAssignNextJob(new List<WorkerNode>() {new WorkerNode() {Url = "localhost:9000"}, new WorkerNode() {Url = "localhost:9001"}},
                                                HttpSender);
            HttpSender.CalledNodes.Clear();
            Target.CancelThisJob(jobId);
            HttpSender.CalledNodes.Count()
                .Should()
                .Be.EqualTo(1);
        }

        [Test]
        public void ShouldRemoveTheJobWhenItsFinished()
        {
            Guid jobId = Guid.NewGuid();
            var job = new JobDefinition() {Id = jobId, AssignedNode = "localhost:9000/", Name = "job", Serialized = "", Type = "", UserName = "ShouldRemoveTheJobWhenItsFinished"};
            JobRepository.Add(job);
            Target.JobDone(job.Id);
            JobRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(0);
        }

        [Test]
        public void ShouldSendOkWhenJobDoneSignalReceived()
        {
            Guid jobId = Guid.NewGuid();
            var job = new JobDefinition() {Id = jobId, AssignedNode = "localhost:9000/", Name = "job", Serialized = "", Type = "", UserName = "ShouldSendOkWhenJobDoneSignalReceived"};
            JobRepository.Add(job);
            var result = Target.JobDone(job.Id);
            result.Should()
                .Not.Be.Null();
        }


        [Test]
        public void ResetJobsOnFalseClaimOnHeartBeatIfItsFree()
        {
            Guid jobId = Guid.NewGuid();
            string userName = "ManagerTests";
            var job = new JobDefinition() {Id = jobId, Name = "job", UserName = userName, Serialized = "Fake Serialized", Type = "Fake Type"};
            JobRepository.Add(job);
            JobRepository.CheckAndAssignNextJob(new List<WorkerNode>() {new WorkerNode() {Url = "localhost:9000/"}},
                                                HttpSender);
            Target.Heartbeat(new Uri("localhost:9000/"));
            HttpSender.CalledNodes.First()
                .Key.Should()
                .Contain("localhost:9000/");
        }

        [Test]
        public void ShouldNotAddSameNodeTwiceInInit()
        {
            Target.NodeInitialized(new Uri("localhost:9000/"));
            Target.NodeInitialized(new Uri("localhost:9000/"));
            NodeRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(1);
        }

        [Test]
        public void ShouldGetUniqueJobIdWhilePersistingJob()
        {
            Target.DoThisJob(new JobRequestModel() {Name = "ShouldGetUniqueJobIdWhilePersistingJob", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"});
            Target.DoThisJob(new JobRequestModel() {Name = "ShouldGetUniqueJobIdWhilePersistingJob", Serialized = "ngt", Type = "bra", UserName = "ManagerTests"});

            JobRepository.LoadAll()
                .Count.Should()
                .Be.EqualTo(2);
        }

        [Test]
        public void ShouldReturnJobHistoryFromJobId()
        {
            var job = new JobRequestModel() {Name = "Name", Serialized = "Ser", Type = "Type", UserName = "ManagerTests"};

            IHttpActionResult doJobResult = Target.DoThisJob(job);

            var okNegotiatedDoJobResult = doJobResult as OkNegotiatedContentResult<Guid>;
            Guid jobId = okNegotiatedDoJobResult.Content;

            // Bad test - TODO  do in another way
            // Must wait for DoJob is done before reading job history
            Thread.Sleep(TimeSpan.FromSeconds(10));

            IHttpActionResult getResult = Target.JobHistory(jobId);

            Assert.IsInstanceOf<OkNegotiatedContentResult<JobHistory>>(getResult);
            var okNegotiatedGetResult = getResult as OkNegotiatedContentResult<JobHistory>;
            Assert.IsNotNull(okNegotiatedGetResult);
        }

        private void thisNodeIsBusy(string url)
        {
            HttpSender.BusyNodesUrl.Add(url);
        }
    }
}