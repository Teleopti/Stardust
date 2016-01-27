using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest
{
    [TestFixture]
    public class NodeControllerTests
    {
        [SetUp]
        public void Setup()
        {
            _nodeConfigurationFake = new NodeConfigurationFake(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
                ConfigurationManager.AppSettings["NodeName"]);

            _workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                _nodeConfigurationFake,
                new NodeStartupNotificationToManagerFake(),
                new PingToManagerFake(),
                new SendJobDoneTimerFake(),
                new SendJobCanceledTimerFake(),
                new SendJobFaultedTimerFake(),
                new PostHttpRequestFake());

            _nodeController = new NodeController(_workerWrapper) {Request = new HttpRequestMessage()};
            var parameters = new TestJobParams("hejhopp",
                "i lingonskogen");
            var ser = JsonConvert.SerializeObject(parameters);
            _JobToDo = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "JobToDo Name",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };
        }

        private NodeConfigurationFake _nodeConfigurationFake;
        private IWorkerWrapper _workerWrapper;
        private NodeController _nodeController;
        private JobToDo _JobToDo;


        [Test]
        public void CancelJobShouldReturnNotFoundWhenCancellingJobWhenIdle()
        {
            var actionResultCancel = _nodeController.TryCancelJob(_JobToDo.Id);
            Assert.IsInstanceOf(typeof (NotFoundResult),
                actionResultCancel);
        }

        [Test]
        public void CancelJobShouldReturnNotFoundWhenCancellingWrongJob()
        {
            var wrongJobToDo = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "Another name",
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            _nodeController.StartJob(_JobToDo);

            var actionResult = _nodeController.TryCancelJob(wrongJobToDo.Id);
            Assert.IsInstanceOf(typeof (NotFoundResult),
                actionResult);
        }

        [Test]
        public void CancelJobShouldReturnOkWhenSuccessful()
        {
            _nodeController.StartJob(_JobToDo);

            var actionResult = _nodeController.TryCancelJob(_JobToDo.Id);
            Assert.IsInstanceOf(typeof (OkResult),
                actionResult);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJobDefinitionIsNullCancelJob()
        {
            _nodeController.TryCancelJob(Guid.Empty);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJobDefinitionIsNullStartJob()
        {
            _nodeController.StartJob(null);
        }

        [Test]
        public void StartJobShouldReturnConflictWhenAlreadyProcessingJob()
        {
            var parameters = new TestJobParams("hejhopp",
                "i lingonskogen");
            var ser = JsonConvert.SerializeObject(parameters);

            var JobToDo2 = new JobToDo {Id = Guid.NewGuid(), Name = "Another name", Serialized = ser};

            _nodeController.StartJob(_JobToDo);

            var actionResult = _nodeController.StartJob(JobToDo2);
            Assert.IsInstanceOf(typeof (ConflictResult),
                actionResult);
        }

        [Test]
        public void StartJobShouldReturnOkIfNotRunningJobAlready()
        {
            var actionResult = _nodeController.StartJob(_JobToDo);
            Assert.IsInstanceOf(typeof (OkNegotiatedContentResult<string>),
                actionResult);
        }
    }
}