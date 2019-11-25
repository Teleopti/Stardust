using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;

using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;
using TimeoutException = System.TimeoutException;

namespace NodeTest
{
	[TestFixture]
	public class WorkerWrapperTests
	{
		[TearDown]
		public void TearDown()
		{
			if (_workerWrapper != null)
			{
				_workerWrapper.Dispose();
			}
		}

		[SetUp]
		public void Setup()
		{
			var parameters = new TestJobParams("Test Job",
			                                   1);

			var ser = JsonConvert.SerializeObject(parameters);

			_jobDefinition = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "jobDefinition Name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};
			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
																				new FakeHttpSender());
			_jobDetailSender  = new JobDetailSender(new FakeHttpSender());
			_trySendJobDetailToManagerTimer =
				new TrySendJobDetailToManagerTimer(_jobDetailSender);

            _trySendJobDetailToManagerTimer.SetupAndStart(_nodeConfigurationFake);
            _pingToManagerFake = new PingToManagerFake();
            _pingToManagerFake.SetupAndStart(_nodeConfigurationFake);

			_sendJobDoneTimer = new SendJobDoneTimerFake(_jobDetailSender,
														 new FakeHttpSender());
            _sendJobDoneTimer.Setup(_nodeConfigurationFake, _nodeConfigurationFake.GetManagerJobDoneTemplateUri());

            _sendJobCanceledTimer = new SendJobCanceledTimerFake(_jobDetailSender,
																 new FakeHttpSender());
            _sendJobCanceledTimer.Setup(_nodeConfigurationFake, _nodeConfigurationFake.GetManagerJobHasBeenCanceledTemplateUri());

            _sendJobFaultedTimer = new SendJobFaultedTimerFake(_jobDetailSender,
															   new FakeHttpSender());
            _sendJobFaultedTimer.Setup(_nodeConfigurationFake, _nodeConfigurationFake.GetManagerJobHasFailedTemplatedUri());

            _now = new MutableNow();
		}

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			_nodeConfigurationFake = new NodeConfiguration(
                new Uri("http://localhost:9001/StardustDashboard/"),
                Assembly.Load(typeof(WorkerModule).Assembly.FullName),
				14100,
				"TestNode",
				60,
				2000,true); 
			CallBackUriTemplateFake = _nodeConfigurationFake.ManagerLocation;
		}

		private Uri CallBackUriTemplateFake { get; set; }
		private NodeConfiguration _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private JobQueueItemEntity _jobDefinition;
		private PingToManagerFake _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
		private JobDetailSender _jobDetailSender;
		private MutableNow _now;

		[Test]
		public void ShouldBeAbleToCatchExceptionsFromJob() //faulting job
		{
			_workerWrapper = new WorkerWrapper(new ThrowExceptionInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
			_workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
            _workerWrapper.IsWorking.Should().Be.True();
            while (true)
            {
                if (!_workerWrapper.IsWorking)
                    break;

                Thread.Sleep(100);
            }
            _workerWrapper.IsWorking.Should().Be.False();
            _sendJobFaultedTimer.AggregateExceptionToSend.InnerExceptions.First().Message.Should().Contain("Exception of type 'System.Exception' was thrown.");
            _sendJobFaultedTimer.ErrorOccured.Should().Not.Be(null);
        }

        [Test]
        public void ShouldHandleStartJobWithNonSerializableJobQueueItem()
        {
            _workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
                _nodeStartupNotification,
                _pingToManagerFake,
                _sendJobDoneTimer,
                _sendJobCanceledTimer,
                _sendJobFaultedTimer,
                _trySendJobDetailToManagerTimer,
                _jobDetailSender,
                _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            var result = _workerWrapper.ValidateStartJob(_jobDefinition);
            _jobDefinition.Serialized = "";
            result.IsWorking.Should().Be.False();
            result.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            _workerWrapper.IsWorking.Should().Be.False();
            _workerWrapper.StartJob(_jobDefinition);
            _workerWrapper.IsWorking.Should().Be.False();
        }

        [Test]
		public void ShouldBeAbleToExecuteJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            var result = _workerWrapper.ValidateStartJob(_jobDefinition);
            result.IsWorking.Should().Be.False();
            result.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            _workerWrapper.IsWorking.Should().Be.False();
            _workerWrapper.StartJob(_jobDefinition);
            _workerWrapper.IsWorking.Should().Be.True();

            var finishedOk = _workerWrapper.Task.Wait(TimeSpan.FromSeconds(5));

            finishedOk.Should().Be.True();
            while (true)
            {
                if (!_workerWrapper.IsWorking)
                    break;

                Thread.Sleep(100);
            }
            _workerWrapper.Task.Status.Should().Be(TaskStatus.RanToCompletion);
        }
		
		[Test]
		public void ShouldReturnIsWorkingWhenJobAssigned()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
				_nodeStartupNotification,
				_pingToManagerFake,
				_sendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, 
				_now);
			_workerWrapper.Init(_nodeConfigurationFake);
			var result = _workerWrapper.ValidateStartJob(_jobDefinition);
			result.IsWorking.Should().Be.False();
			result.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			var result2 = _workerWrapper.ValidateStartJob(_jobDefinition);
			result2.IsWorking.Should().Be.True();
			result2.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Test]
		public void ShouldBeAbleToTryCancelJob()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
			Assert.IsTrue(_workerWrapper.IsWorking);
			_workerWrapper.CancelJob(_jobDefinition.JobId);
			
			Assert.IsTrue(_workerWrapper.IsCancellationRequested);


			var timeoutWaitingForIdle = DateTime.Now.AddSeconds(10);
			while (_workerWrapper.IsWorking)
			{
				Thread.Sleep(100);
				if(DateTime.Now > timeoutWaitingForIdle)
					throw new TimeoutException("Timed out waiting for Node to become Idle in test");
			}
			
			_workerWrapper.IsWorking.Should().Be.False();
		}

		[Test]
		public void ShouldNotThrowWhenCancellingAlreadyCancelledJob()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
			_workerWrapper.CancelJob(_jobDefinition.JobId);

			_workerWrapper.CancelJob(_jobDefinition.JobId);

			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIdIsEmptyGuid()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            var actionResult = _workerWrapper.ValidateStartJob(new JobQueueItemEntity() { JobId = Guid.Empty });
			Assert.IsTrue(actionResult.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsEmpty()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            var actionResult = _workerWrapper.ValidateStartJob(new JobQueueItemEntity());
			Assert.IsTrue(actionResult.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsNull()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            var actionResult = _workerWrapper.ValidateStartJob(null);
			Assert.IsTrue(actionResult.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest);
		}
		
		[Test]
		public void ShouldSendJobDone()
		{
			var fakeHttpSender = new FakeHttpSender();
			var newSendJobDoneTimer = new TrySendJobDoneStatusToManagerTimer(_jobDetailSender, fakeHttpSender);
			var jobDoneTimerTrigger = new ManualResetEvent(false);
			newSendJobDoneTimer.TrySendStatusSucceded += (sender, args) => { jobDoneTimerTrigger.Set(); };
			newSendJobDoneTimer.Setup(_nodeConfigurationFake,CallBackUriTemplateFake);
			var startTime = new DateTime(2019,5,13,15,22,0);
			_now.Is(startTime);
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(), 
				_nodeStartupNotification,
				_pingToManagerFake,
				newSendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, 
				_now);
			_workerWrapper.Init(_nodeConfigurationFake);
			var validateStartJobResult = _workerWrapper.ValidateStartJob(_jobDefinition);
			validateStartJobResult.IsWorking.Should().Be.False();
			validateStartJobResult.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			
			_workerWrapper.StartJob(_jobDefinition);
			while( _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_now.Is(startTime.AddMinutes(6)); // faking long job by increasing now time 
			
			_workerWrapper.Task.Wait();
			var triggeredOk = jobDoneTimerTrigger.WaitOne(2000);

			triggeredOk.Should().Be.True();
			fakeHttpSender.CalledUrl.Should().Contain(CallBackUriTemplateFake+"status/done");
			_workerWrapper.IsWorking.Should().Be.False();
		}
		
		[Test]
		public void ShouldReceiveJobDoneOnJobsTakingLongerThan5Minutes()
		{
			var fakeHttpSender = new FakeHttpSender();
			var newSendJobDoneTimer = new TrySendJobDoneStatusToManagerTimer(_jobDetailSender, fakeHttpSender);
			var jobDoneTimerTrigger = new ManualResetEvent(false);
			newSendJobDoneTimer.TrySendStatusSucceded += (sender, args) => { jobDoneTimerTrigger.Set(); };
			newSendJobDoneTimer.Setup(_nodeConfigurationFake,CallBackUriTemplateFake);
			var startTime = new DateTime(2019,5,13,15,22,0);
			_now.Is(startTime);
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(), 
				_nodeStartupNotification,
				_pingToManagerFake,
				newSendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, 
				_now);
			_workerWrapper.Init(_nodeConfigurationFake);
			var validateStartJobResult = _workerWrapper.ValidateStartJob(_jobDefinition);
			validateStartJobResult.IsWorking.Should().Be.False();
			validateStartJobResult.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			_workerWrapper.StartJob(_jobDefinition);
			while( _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_now.Is(startTime.AddMinutes(6)); // faking long job by increasing now time 
			var validateStartJobResult2 = _workerWrapper.ValidateStartJob(_jobDefinition);			
			
			_workerWrapper.Task.Wait();
			var triggeredOk = jobDoneTimerTrigger.WaitOne(2000);

			validateStartJobResult2.IsWorking.Should().Be.True();
			validateStartJobResult2.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			
			triggeredOk.Should().Be.True();
			fakeHttpSender.CalledUrl.Should().Contain(CallBackUriTemplateFake+"status/done");
			_workerWrapper.IsWorking.Should().Be.False();
		}
		
		[Test]
		public void ShouldReceiveJobFailedOnJobsTakingLongerThan5MinutesAndFailing()
		{
			var fakeHttpSender = new FakeHttpSender();
			var newSendJobFailedTimer = new TrySendJobFaultedToManagerTimer(_jobDetailSender, fakeHttpSender);
			var jobFailedTimerTrigger = new ManualResetEvent(false);
			newSendJobFailedTimer.TrySendStatusSucceded += (sender, args) => { jobFailedTimerTrigger.Set(); };
			newSendJobFailedTimer.Setup(_nodeConfigurationFake,CallBackUriTemplateFake);
			var startTime = new DateTime(2019,5,13,15,22,0);
			_now.Is(startTime);
			_workerWrapper = new WorkerWrapper(new ThrowExceptionInvokeHandlerFake(), 
				_nodeStartupNotification,
				_pingToManagerFake,
				_sendJobDoneTimer,
				_sendJobCanceledTimer,
				newSendJobFailedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, 
				_now);
			_workerWrapper.Init(_nodeConfigurationFake);
			var validateStartJobResult = _workerWrapper.ValidateStartJob(_jobDefinition);
			validateStartJobResult.IsWorking.Should().Be.False();
			validateStartJobResult.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			_workerWrapper.StartJob(_jobDefinition);
			while( _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_now.Is(startTime.AddMinutes(6)); // faking long job by increasing now time 
			var validateStartJobResult2 = _workerWrapper.ValidateStartJob(_jobDefinition);			
			
			while( _workerWrapper.Task.Status == TaskStatus.Running) { Thread.Sleep(100); }
			var triggeredOk = jobFailedTimerTrigger.WaitOne(5000);

			validateStartJobResult2.IsWorking.Should().Be.True();
			validateStartJobResult2.HttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
			
			triggeredOk.Should().Be.True();
			fakeHttpSender.CalledUrl.Should().Contain(CallBackUriTemplateFake+"status/fail");
			_workerWrapper.GetCurrentMessageToProcess().Should().Be.Null();
			_workerWrapper.IsWorking.Should().Be.False();
		}

	}
}