using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Shared.Stardust.Node;
using SharpTestsEx;
using Stardust.Node;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest
{
	[TestFixture]
	public class NodeControllerTests
	{

        private WorkerWrapperServiceFake _workerWrapperService;
        private NodeConfiguration _nodeConfigurationFake;
        private IWorkerWrapper _workerWrapper;
        private JobQueueItemEntity _jobQueueItemEntity;
        private IPingToManagerTimer _pingToManagerFake;
        private NodeStartupNotificationToManagerFake _nodeStartupNotification;
        private SendJobDoneTimerFake _sendJobDoneTimer;
        private SendJobCanceledTimerFake _sendJobCanceledTimer;
        private SendJobFaultedTimerFake _sendJobFaultedTimer;
        private TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
        private JobDetailSender _jobDetailSender;
        private MutableNow _now;
        private NodeActionExecutor Target;

        private const int PortNumber = 14100;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            _nodeConfigurationFake = new NodeConfiguration(
                new Uri("http://localhost:9001/StardustDashboard/"),
                Assembly.Load(typeof(WorkerModule).Assembly.FullName),
                PortNumber,
                "TestNode",
                60,
                2000, true);

            _workerWrapperService = new WorkerWrapperServiceFake();
        }

        [SetUp]
		public void SetUp()
		{
			var parameters = new TestJobParams("Test Job", 20);
            var ser = JsonConvert.SerializeObject(parameters);
            _jobQueueItemEntity = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "JobToDo Name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake, new FakeHttpSender());
			_pingToManagerFake = new PingToManagerFake();
            _jobDetailSender = new JobDetailSender(new FakeHttpSender());
            _trySendJobDetailToManagerTimer = new TrySendJobDetailToManagerTimer(_jobDetailSender);
            _trySendJobDetailToManagerTimer.SetupAndStart(_nodeConfigurationFake);
            _sendJobDoneTimer = new SendJobDoneTimerFake(_jobDetailSender, new FakeHttpSender());
            _sendJobCanceledTimer = new SendJobCanceledTimerFake(_jobDetailSender, new FakeHttpSender());
            _sendJobFaultedTimer = new SendJobFaultedTimerFake(_jobDetailSender, new FakeHttpSender());
			_now = new MutableNow();
        }

		[Test]
		public void CancelJobShouldReturnNotFoundWhenNodeIsIdle()
		{
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var response = Target.TryCancelJob(_jobQueueItemEntity.JobId, PortNumber);
            response.Result.Should().Be(SimpleResponse.Status.NotFound);
        }

		[Test]
		public void CancelJobShouldReturnNotFoundWhenCancellingWrongJob()
		{
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
            Target.StartJob(_jobQueueItemEntity.JobId, PortNumber);
			
			var wrongJobId = Guid.NewGuid();
			var response = Target.TryCancelJob(wrongJobId, PortNumber);

            response.Result.Should().Be(SimpleResponse.Status.NotFound);
        }

        [Test]
        public void CancelJobShouldReturnOkWhenSuccessful()
        {
            SetupWorkerWrapperWithHandler(new LongRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
            Target.StartJob(_jobQueueItemEntity.JobId, PortNumber);

            while (!_workerWrapper.IsWorking)
            {
                Thread.Sleep(200);
            }
            var actionResult = Target.TryCancelJob(_jobQueueItemEntity.JobId, PortNumber);
            actionResult.Result.Should().Be(SimpleResponse.Status.Ok);
        }

        [Test]
        public void ShouldReturnBadRequestWhenCancelJobWithNoId()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var actionResult = Target.TryCancelJob(Guid.Empty, PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.BadRequest);
        }

        [Test]
        public void ShouldReturnBadRequestWhenPrepareToStartJobWithNoJob()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var actionResult = Target.PrepareToStartJob(null, PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.BadRequest);
            actionResult.Message.Should().Be("Job to do can not be null.");
        }


        [Test]
        public void PrepareToStartJobShouldReturnIsWorkingWhenAlreadyProcessingJob()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var parameters = new TestJobParams("Test Job", 1);
            var ser = JsonConvert.SerializeObject(parameters);
            var jobToDo2 = new JobQueueItemEntity
            {
                JobId = Guid.NewGuid(),
                Name = "Another name",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            var actionResult1 = Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
          
            actionResult1.Result.Should().Be(SimpleResponse.Status.Ok);
            var prepareToStartJobResult = JsonConvert.DeserializeObject<PrepareToStartJobResult>(actionResult1.ResponseValue);
            prepareToStartJobResult.IsAvailable.Should().Be.True();

            var actionResult2 = Target.PrepareToStartJob(jobToDo2, PortNumber);
            actionResult2.Result.Should().Be(SimpleResponse.Status.Ok);
            var prepareToStartJobResult2 = JsonConvert.DeserializeObject<PrepareToStartJobResult>(actionResult2.ResponseValue);
            prepareToStartJobResult2.IsAvailable.Should().Be.False();
        }

        [Test]
        public void PrepareToStartJobShouldReturnOkIfNotRunningJobAlready()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var actionResult = Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.Ok);

        }


        [Test]
        public void StartJobShouldReturnBadRequestWhenStartJobIdDoesNotMatchPrepareJobId()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);

            var actionResult = Target.StartJob(Guid.NewGuid(), PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.BadRequest);
        }


        [Test]
        public void StartJobShouldReturnOkIfJobIdMatchPrepareJobId()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
            var actionResult = Target.StartJob(_jobQueueItemEntity.JobId, PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.Ok);
        }

        [Test]
        public void ShouldTimeoutIfWaitingMoreThan5Minutes()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var parameters = new TestJobParams("Test Job", 1);
            var ser = JsonConvert.SerializeObject(parameters);

            var jobToDo2 = new JobQueueItemEntity
            {
                JobId = Guid.NewGuid(),
                Name = "Another name",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            _now.Is(new DateTime(2019, 2, 28, 10, 10, 0));
            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
            _workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();

            _now.Is(_now.UtcDateTime().AddMinutes(6));
            var actionResult = Target.PrepareToStartJob(jobToDo2, PortNumber);

            actionResult.Result.Should().Be(SimpleResponse.Status.Ok);
            _workerWrapper.IsWorking.Should().Be.False();
        }

        [Test]
        public void ShouldNotTimeoutIfWaitingLessThan5Minutes()
        {
            SetupWorkerWrapperWithHandler(new ShortRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);

            var parameters = new TestJobParams("Test Job", 1);
            var ser = JsonConvert.SerializeObject(parameters);

            var jobToDo2 = new JobQueueItemEntity
            {
                JobId = Guid.NewGuid(),
                Name = "Another name",
                Serialized = ser,
                Type = "NodeTest.JobHandlers.TestJobParams"
            };

            _now.Is(new DateTime(2019, 2, 28, 10, 10, 0));
            Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);

            _now.Is(_now.UtcDateTime().AddMinutes(3));
            var response = Target.PrepareToStartJob(jobToDo2, PortNumber);
            response.Result.Should().Be(SimpleResponse.Status.Ok);
            JsonConvert.DeserializeObject<PrepareToStartJobResult>(response.ResponseValue).IsAvailable.Should().Be.False();
            _workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
        }

        [Test]
		public void ShouldNotTimeoutIfJobIsWorking()
        {
            SetupWorkerWrapperWithHandler( new LongRunningInvokeHandlerFake());
            Target = new NodeActionExecutor(_workerWrapperService);


			var serializedParameters = JsonConvert.SerializeObject(new TestJobParams("Test Job", 1));
			var jobToDo2 = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "Another name",
				Serialized = serializedParameters,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_now.Is(new DateTime(2019, 2, 28, 10, 10, 0));
            var prepareToStartJobResult = Target.PrepareToStartJob(_jobQueueItemEntity, PortNumber);
            prepareToStartJobResult.Result.Should().Be(SimpleResponse.Status.Ok);
			var prepareResultOk = JsonConvert.DeserializeObject<PrepareToStartJobResult>(prepareToStartJobResult.ResponseValue);
			prepareResultOk.IsAvailable.Should().Be.True();
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			_workerWrapper.IsWorking.Should().Be.False();

            var startJobResult = Target.StartJob(_jobQueueItemEntity.JobId, PortNumber);
				
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			startJobResult.Result.Should().Be(SimpleResponse.Status.Ok);

            while (_workerWrapper.Task == null || _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_workerWrapper.IsWorking.Should().Be.True();
			_now.Is(_now.UtcDateTime().AddMinutes(6));
			_workerWrapper.IsWorking.Should().Be.True();

            var prepareToStartJobResult2 = Target.PrepareToStartJob(jobToDo2, PortNumber);
            prepareToStartJobResult2.Result.Should().Be(SimpleResponse.Status.Ok);
            var prepareResult2Ok = JsonConvert.DeserializeObject<PrepareToStartJobResult>(prepareToStartJobResult2.ResponseValue);
			prepareResult2Ok.IsAvailable.Should().Be.False();
			
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
		}


        private void SetupWorkerWrapperWithHandler(IInvokeHandler handler)
        {
            _workerWrapper = new WorkerWrapper(handler,
                _nodeStartupNotification,
                _pingToManagerFake,
                _sendJobDoneTimer,
                _sendJobCanceledTimer,
                _sendJobFaultedTimer,
                _trySendJobDetailToManagerTimer,
                _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

        }
    }
}