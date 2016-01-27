using System;
using System.Net.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NodeTest.Attributes;
using NodeTest.Fakes;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node.API;
using Stardust.Node.Interfaces;

namespace NodeTest
{
    [TestFixture, ProgressTest]
    public class ProgressCallbackTests
    {
        public NodeController NodeController;
        public PostHttpRequestFake PostHttpRequest;
        public SendJobFaultedTimerFake FaultedTimer;
        public SendJobCanceledTimerFake CanceledTimer;
        public SendJobDoneTimerFake DoneTimer;

        [Test]
        public void OnCancelCanceledShouldBeCalled()
        {
            var jobParams = new TestJobParams("tjo", "flöjt");
            var ser = JsonConvert.SerializeObject(jobParams);
            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };
            CanceledTimer.JobToDo = job;
            CanceledTimer.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            NodeController.StartJob(job);

            NodeController.TryCancelJob(job.Id);
            CanceledTimer.Wait.Wait();
            DoneTimer.NumberOfTimeCalled.Should().Be.EqualTo(0);
            CanceledTimer.NumberOfTimeCalled.Should().Be.GreaterThan(0);
        }

        [Test]
        public void OnErrorFaultedShouldBeCalled()
        {
            var jobParams = new FailingJobParams("mama mia");
            var ser = JsonConvert.SerializeObject(jobParams);
            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.FailingJobParams"
            };
            FaultedTimer.JobToDo = job;
            FaultedTimer.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            NodeController.StartJob(job);

            FaultedTimer.Wait.Wait();
            FaultedTimer.NumberOfTimeCalled.Should().Be.GreaterThan(0);
        }

        [Test]
        public void OnWrongSerializedBadRequestShouldBeReturned()
        {
            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = "rappakalja",
                Type = "NodeTest.JobHandlers.TestJobParams"
            };
            CanceledTimer.JobToDo = job;
            CanceledTimer.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            var result = NodeController.StartJob(job);
            result.Should().Be.OfType<BadRequestResult>();
        }

        [Test]
        public void OnWrongTypeBadRequestShouldBeReturned()
        {
            var jobParams = new TestJobParams("tjo", "flöjt");
            var ser = JsonConvert.SerializeObject(jobParams);
            var job = new JobToDo {Id = Guid.NewGuid(), Name = "Janne", Serialized = ser, Type = "rappakalja"};
            CanceledTimer.JobToDo = job;
            CanceledTimer.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            var result = NodeController.StartJob(job);
            result.Should().Be.OfType<BadRequestResult>();
        }

        [Test]
        public void ProgressShouldBeSentToManager()
        {
            var jobParams = new TestJobParams("tjo", "flöjt");
            var ser = JsonConvert.SerializeObject(jobParams);
            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };
            NodeController.Request = new HttpRequestMessage();
            NodeController.StartJob(job);

            PostHttpRequest.CalledUrl.Should().Not.Be.Empty();
        }

        [Test]
        public void ShouldContainController()
        {
            NodeController.Should().Not.Be.Null();
        }
    }
}