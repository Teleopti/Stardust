using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node;
using Stardust.Node.API;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest
{
    [TestFixture]
    public class WorkerWrapperTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);
            var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);
            var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);
            var nodeName = ConfigurationManager.AppSettings["NodeName"];

            CallBackUriTemplateFake = managerLocation;

            NodeConfigurationFake = new NodeConfigurationFake(baseAddress,
                                                              managerLocation,
                                                              handlerAssembly,
                                                              nodeName);

            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

        }

        [SetUp]
        public void Setup()
        {
            WorkerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                                  NodeConfigurationFake,
                                  new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                           CallBackUriTemplateFake),
                                  new PingToManagerFake(),
                                  new SendJobDoneTimerFake(NodeConfigurationFake,
                                                           CallBackUriTemplateFake),
                                  new SendJobCanceledTimerFake(NodeConfigurationFake,
                                                               CallBackUriTemplateFake),
                                  new SendJobFaultedTimerFake(NodeConfigurationFake,
                                                              CallBackUriTemplateFake),
                                  new PostHttpRequestFake());

            NodeController = new NodeController(WorkerWrapper);

            var parameters = new TestJobParams("hejhopp",
                                               "i lingonskogen");

            var ser = JsonConvert.SerializeObject(parameters);

            JobDefinition = new JobToDo
            {
                Id = Guid.NewGuid(),
                Name = "jobDefinition Name",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.LogInfoWithLineNumber(Logger, "Closing WorkerWrapperTests...");
        }

        private Uri CallBackUriTemplateFake { get; set; }
        public NodeConfigurationFake NodeConfigurationFake;
        public IWorkerWrapper WorkerWrapper;
        public NodeController NodeController;
        public JobToDo JobDefinition;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WorkerWrapperTests));

        [Test]
        public void ShouldBeAbleToCatchExceptionsFromJob() //faulting job
        {

            var httpRequestMessage = new HttpRequestMessage();
            // Start job.
            var actionResult = WorkerWrapper.StartJob(JobDefinition,
                                                      httpRequestMessage);

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void ShouldBeAbleToStartJob()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                                                      CallBackUriTemplateFake),
                                                             new PingToManagerFake(),
                                                             new SendJobDoneTimerFake(NodeConfigurationFake,
                                                                                      CallBackUriTemplateFake),
                                                             new SendJobCanceledTimerFake(NodeConfigurationFake,
                                                                                          CallBackUriTemplateFake),
                                                             new SendJobFaultedTimerFake(NodeConfigurationFake,
                                                                                         CallBackUriTemplateFake),
                                                             new PostHttpRequestFake());


            var httpRequestMessage = new HttpRequestMessage();

            var actionResult = workerWrapper.StartJob(JobDefinition,
                                                      httpRequestMessage);

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void ShouldBeAbleToTryCancelJob()
        {
            var httpRequestMessage = new HttpRequestMessage();

            //-------------------------------------------
            // Start a job.
            //-------------------------------------------
            var actionResult = WorkerWrapper.StartJob(JobDefinition,
                                                      httpRequestMessage);
            //-------------------------------------------
            // Try cancel job.
            //-------------------------------------------
            WorkerWrapper.CancelJob(JobDefinition.Id);

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(WorkerWrapper.IsCancellationRequested);
        }

        [Test]
        public void ShouldNotThrowWhenCancellingAlreadyCancelledJob()
        {
            var httpRequestMessage = new HttpRequestMessage();

            //-------------------------------------------
            // Start a job.
            //-------------------------------------------
            var actionResult = WorkerWrapper.StartJob(JobDefinition,
                                                      httpRequestMessage);
            //-------------------------------------------
            // Try cancel job.
            //-------------------------------------------
            WorkerWrapper.CancelJob(JobDefinition.Id);

            actionResult.ExecuteAsync(new CancellationToken());

            //-------------------------------------------
            // Try cancel job.
            //-------------------------------------------
            WorkerWrapper.CancelJob(JobDefinition.Id);

            Assert.IsTrue(WorkerWrapper.IsCancellationRequested);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenInvokeHandlerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJobCanceledTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                                                      CallBackUriTemplateFake),
                                                             new PingToManagerFake(),
                                                             new SendJobDoneTimerFake(NodeConfigurationFake,
                                                                                      CallBackUriTemplateFake),
                                                             null,
                                                             null,
                                                             new PostHttpRequestFake());
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJobFaultedTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                                                      CallBackUriTemplateFake),
                                                             new PingToManagerFake(),
                                                             new SendJobDoneTimerFake(NodeConfigurationFake,
                                                                                      CallBackUriTemplateFake),
                                                             new SendJobCanceledTimerFake(NodeConfigurationFake,
                                                                                          CallBackUriTemplateFake),
                                                             null,
                                                             new PostHttpRequestFake());
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJodDoneTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                                                      CallBackUriTemplateFake),
                                                             new PingToManagerFake(),
                                                             null,
                                                             null,
                                                             null,
                                                             new PostHttpRequestFake());
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenNodeConfigurationIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             new PostHttpRequestFake());
        }


        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenNodeStartupNotificationToManaagerTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             new PostHttpRequestFake());
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenPingToManagerTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                                                             NodeConfigurationFake,
                                                             new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                                                                                                      CallBackUriTemplateFake),
                                                             null,
                                                             null,
                                                             null,
                                                             null,
                                                             new PostHttpRequestFake());
        }

        [Test]
        public void StartJobShouldReturnBadRequestWhenMessageIdIsEmptyGuid()
        {
            var actionResult = WorkerWrapper.StartJob(new JobToDo(),
                                                      new HttpRequestMessage());

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode ==
                          HttpStatusCode.BadRequest);
        }

        [Test]
        public void StartJobShouldReturnBadRequestWhenMessageIsEmpty()
        {
            var actionResult = WorkerWrapper.StartJob(new JobToDo(),
                                                      new HttpRequestMessage());

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode ==
                          HttpStatusCode.BadRequest);
        }

        [Test]
        public void StartJobShouldReturnBadRequestWhenMessageIsNull()
        {
            var actionResult = WorkerWrapper.StartJob(null,
                                                      new HttpRequestMessage());

            Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
                              .Result.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}