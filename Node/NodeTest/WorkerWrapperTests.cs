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
				new TrySendJobDetailToManagerTimer(_nodeConfigurationFake,
														_jobDetailSender);

			_pingToManagerFake = new PingToManagerFake();

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
														 _jobDetailSender,
														 new FakeHttpSender());

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
																 _jobDetailSender,
																 new FakeHttpSender());

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
															   _jobDetailSender,
															   new FakeHttpSender());
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
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
			_workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
		}

		[Test]
		public void ShouldBeAbleToStartJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
			_workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
		}

		[Test]
		public void ShouldBeAbleToTryCancelJob()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
			_workerWrapper.ValidateStartJob(_jobDefinition);
			_workerWrapper.StartJob(_jobDefinition);
			_workerWrapper.CancelJob(_jobDefinition.JobId);

			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
		}

		[Test]
		public void ShouldNotThrowWhenCancellingAlreadyCancelledJob()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
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
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);
			
			var actionResult = _workerWrapper.ValidateStartJob(new JobQueueItemEntity() { JobId = Guid.Empty });
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsEmpty()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);

			var actionResult = _workerWrapper.ValidateStartJob(new JobQueueItemEntity());
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsNull()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
											   _now);

			var actionResult = _workerWrapper.ValidateStartJob(null);
			Assert.IsTrue(actionResult.StatusCode == HttpStatusCode.BadRequest);
		}

	}
}