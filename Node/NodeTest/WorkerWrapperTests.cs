using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.ElapsedEventHandlers;
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
    public class WorkerWrapperTests
    {
        [SetUp]
        public void Setup()
        {
            NodeConfigurationFake = new NodeConfigurationFake(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
                ConfigurationManager.AppSettings["NodeName"]);

            WorkerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                NodeConfigurationFake,
                new NodeStartupNotificationToManagerFake(),
                new PingToManagerFake(),
                new SendJobDoneTimerFake(),
                new SendJobCanceledTimerFake(),
                new SendJobFaultedTimerFake(),
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

        private bool IsHandler(Type arg)
        {
            return arg.GetInterfaces()
                .Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof (IHandle<>));
        }

        public NodeConfigurationFake NodeConfigurationFake;
        public IWorkerWrapper WorkerWrapper;
        public NodeController NodeController;
        public JobToDo JobDefinition;

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
            var elapsedEventHandlersFake = new ElapsedEventHandlersFake();

            IWorkerWrapper workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                NodeConfigurationFake,
                new NodeStartupNotificationToManagerFake(NodeConfigurationFake,
                    null,
                    elapsedEventHandlersFake.NodeStartupElapsedEventHandler),
                new PingToManagerFake(),
                new SendJobDoneTimerFake(null,
                    NodeConfigurationFake,
                    null,
                    elapsedEventHandlersFake.SendJodDoneElapsedEventHandler),
                new SendJobCanceledTimerFake(null,
                    NodeConfigurationFake,
                    null,
                    elapsedEventHandlersFake.SendJodCanceledElapsedEventHandler),
                new SendJobFaultedTimerFake(null,
                    NodeConfigurationFake,
                    null,
                    elapsedEventHandlersFake.SendJodFaultedElapsedEventHandler),
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
                new NodeStartupNotificationToManagerFake(),
                new PingToManagerFake(),
                new SendJobDoneTimerFake(),
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
                new NodeStartupNotificationToManagerFake(),
                new PingToManagerFake(),
                new SendJobDoneTimerFake(),
                new SendJobCanceledTimerFake(),
                null,
                new PostHttpRequestFake());
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionWhenJodDoneTimerIsNull()
        {
            IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
                NodeConfigurationFake,
                new NodeStartupNotificationToManagerFake(),
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
                new NodeStartupNotificationToManagerFake(),
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