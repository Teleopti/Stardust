using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

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

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_nodeConfigurationFake = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
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
		}

		[Test]
		public void ShouldBeAbleToStartJob()
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
            _workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
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
			_workerWrapper.CancelJob(_jobDefinition.JobId);

			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
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
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
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
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
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
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
		}

	}
}