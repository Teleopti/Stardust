using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Results;
using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using NodeTest.Attributes;
using NodeTest.Fakes;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest
{
    [TestFixture, ProgressTest, Ignore]
    public class ProgressCallbackTests
    {
        public NodeController NodeController;

        public PostHttpRequestFake PostHttpRequestFake;
        public SendJobFaultedTimerFake SendJobFaultedTimerFake;
        public SendJobCanceledTimerFake SendJobCanceledTimerFake;
        public SendJobDoneTimerFake SendJobDoneTimerFake;
                        
        [Test]
        public void OnCancelCanceledShouldBeCalled()
        {
            var jobParams = new TestJobParams("tjo",
                "flöjt");

            var ser = JsonConvert.SerializeObject(jobParams);

            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            SendJobCanceledTimerFake.JobToDo = job;
            SendJobCanceledTimerFake.Interval = 100;

            NodeController.Request = new HttpRequestMessage();
            //   NodeController.StartJob(job);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            NodeController.TryCancelJob(job.Id);

            SendJobCanceledTimerFake.Wait.Wait(TimeSpan.FromMinutes(2));

            SendJobDoneTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
            SendJobCanceledTimerFake.NumberOfTimeCalled.Should().Be.GreaterThan(0);
            SendJobFaultedTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
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
            SendJobFaultedTimerFake.JobToDo = job;
            SendJobFaultedTimerFake.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            NodeController.StartJob(job);

            SendJobFaultedTimerFake.Wait.Wait(TimeSpan.FromMinutes(2));

            SendJobFaultedTimerFake.NumberOfTimeCalled.Should()
                .Be.GreaterThan(0);
            SendJobDoneTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
            SendJobCanceledTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
        }

        [Test]
        public void OnSuccessSuccededShouldBeCalled()
        {
            var jobParams = new TestJobParams("tjo",
                "flöjt");

            var ser = JsonConvert.SerializeObject(jobParams);

            var job = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Janne",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            SendJobDoneTimerFake.JobToDo = job;
            SendJobDoneTimerFake.Interval = 100;

            NodeController.Request = new HttpRequestMessage();
            NodeController.StartJob(job);

            SendJobDoneTimerFake.Wait.Wait(TimeSpan.FromMinutes(2));

            SendJobDoneTimerFake.NumberOfTimeCalled.Should().Be.GreaterThan(0);
            SendJobCanceledTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
            SendJobFaultedTimerFake.NumberOfTimeCalled.Should().Be.EqualTo(0);
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
            SendJobCanceledTimerFake.JobToDo = job;
            SendJobCanceledTimerFake.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            var result = NodeController.StartJob(job);
            result.Should()
                .Be.OfType<BadRequestResult>();
        }

        [Test]
        public void OnWrongTypeBadRequestShouldBeReturned()
        {
            var jobParams = new TestJobParams("tjo",
                "flöjt");
            var ser = JsonConvert.SerializeObject(jobParams);
            var job = new JobToDo {Id = Guid.NewGuid(), Name = "Janne", Serialized = ser, Type = "rappakalja"};
            SendJobCanceledTimerFake.JobToDo = job;
            SendJobCanceledTimerFake.Interval = 100;
            NodeController.Request = new HttpRequestMessage();
            var result = NodeController.StartJob(job);
            result.Should()
                .Be.OfType<BadRequestResult>();
        }

        [Test]
        public void ProgressShouldBeSentToManager()
        {
            var jobParams = new TestJobParams("tjo",
                "flöjt");
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

            PostHttpRequestFake.CalledUrl.Should()
                .Not.Be.Empty();
        }

        [Test]
        public void ShouldContainController()
        {
            NodeController.Should()
                .Not.Be.Null();
        }
    }
}