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
            var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

            var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

            var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

            var nodeName = ConfigurationManager.AppSettings["NodeName"];

            _nodeConfigurationFake = new NodeConfigurationFake(baseAddress,
                                                               managerLocation,
                                                               handlerAssembly,
                                                               nodeName);


            _callBackTemplateUriFake = managerLocation;

            _workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                                               _nodeConfigurationFake,
                                               new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
                                                                                        _callBackTemplateUriFake),
                                               new PingToManagerFake(),
                                               new SendJobDoneTimerFake(_nodeConfigurationFake,
                                                                        _callBackTemplateUriFake),
                                               new SendJobCanceledTimerFake(_nodeConfigurationFake,
                                                                            _callBackTemplateUriFake),
                                               new SendJobFaultedTimerFake(_nodeConfigurationFake,
                                                                           _callBackTemplateUriFake),
                                               new PostHttpRequestFake());

            _nodeController = new NodeController(_workerWrapper) {Request = new HttpRequestMessage()};
            var parameters = new TestJobParams("hejhopp",
                                               "i lingonskogen");
            var ser = JsonConvert.SerializeObject(parameters);
            _jobToDo = new JobToDo
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
        private JobToDo _jobToDo;
        private Uri _callBackTemplateUriFake;


        [Test]
        public void CancelJobShouldReturnNotFoundWhenCancellingJobWhenIdle()
        {
            var actionResultCancel = _nodeController.TryCancelJob(_jobToDo.Id);
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

            _nodeController.StartJob(_jobToDo);

            var actionResult = _nodeController.TryCancelJob(wrongJobToDo.Id);
            Assert.IsInstanceOf(typeof (NotFoundResult),
                                actionResult);
        }

        [Test]
        public void CancelJobShouldReturnOkWhenSuccessful()
        {
            _nodeController.StartJob(_jobToDo);

            var actionResult = _nodeController.TryCancelJob(_jobToDo.Id);
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

            _nodeController.StartJob(_jobToDo);

            var actionResult = _nodeController.StartJob(JobToDo2);
            Assert.IsInstanceOf(typeof (ConflictResult),
                                actionResult);
        }

        [Test]
        public void StartJobShouldReturnOkIfNotRunningJobAlready()
        {
            var actionResult = _nodeController.StartJob(_jobToDo);
            Assert.IsInstanceOf(typeof (OkNegotiatedContentResult<string>),
                                actionResult);
        }
    }
}