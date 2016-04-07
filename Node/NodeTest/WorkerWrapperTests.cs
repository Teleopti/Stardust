using System;
using System.Configuration;
using System.Reflection;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
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
			var parameters = new TestJobParams("hejhopp",
			                                   "i lingonskogen");

			var ser = JsonConvert.SerializeObject(parameters);

			_jobDefinition = new JobToDo
			{
				Id = Guid.NewGuid(),
				Name = "jobDefinition Name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};
			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
																				new FakeHttpSender());

			_trySendJobProgressToManagerTimerFake =
				new TrySendJobProgressToManagerTimerFake(_nodeConfigurationFake,
														new FakeHttpSender(),
														1000);

			_pingToManagerFake = new PingToManagerFake();

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
														 _trySendJobProgressToManagerTimerFake,
														 new FakeHttpSender());

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
																 _trySendJobProgressToManagerTimerFake,
																 new FakeHttpSender());

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
															   _trySendJobProgressToManagerTimerFake,
															   new FakeHttpSender());

		}

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);
			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);
			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			var pingToManagerSeconds =
				Convert.ToDouble(ConfigurationManager.AppSettings["PingToManagerSeconds"]);

			CallBackUriTemplateFake = managerLocation;

			_nodeConfigurationFake = new NodeConfiguration(baseAddress,
			                                                   managerLocation,
			                                                   handlerAssembly,
			                                                   nodeName,
															   pingToManagerSeconds);
		}

		private Uri CallBackUriTemplateFake { get; set; }
		private NodeConfiguration _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private JobToDo _jobDefinition;
		private PingToManagerFake _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobProgressToManagerTimerFake _trySendJobProgressToManagerTimerFake;

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
											   _trySendJobProgressToManagerTimerFake);

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
											   _trySendJobProgressToManagerTimerFake);

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
											   _trySendJobProgressToManagerTimerFake);

			_workerWrapper.StartJob(_jobDefinition);
			_workerWrapper.CancelJob(_jobDefinition.Id);

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
											   _trySendJobProgressToManagerTimerFake);

			_workerWrapper.StartJob(_jobDefinition);
			_workerWrapper.CancelJob(_jobDefinition.Id);

			_workerWrapper.CancelJob(_jobDefinition.Id);

			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenInvokeHandlerIsNull()
		{
			new WorkerWrapper(null, null, null, null, null, null, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJobCanceledTimerIsNull()
		{
			new WorkerWrapper(new InvokeHandlerFake(), _nodeConfigurationFake, _nodeStartupNotification,  _pingToManagerFake, _sendJobDoneTimer, null, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJobFaultedTimerIsNull()
		{
			new WorkerWrapper(new InvokeHandlerFake(), _nodeConfigurationFake, _nodeStartupNotification, _pingToManagerFake,
				_sendJobDoneTimer, _sendJobCanceledTimer, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJodDoneTimerIsNull()
		{
			new WorkerWrapper(new InvokeHandlerFake(), _nodeConfigurationFake, _nodeStartupNotification, _pingToManagerFake, null, null, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeConfigurationIsNull()
		{
			new WorkerWrapper(new InvokeHandlerFake(), null, null, null,  null, null, null, null);
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeStartupNotificationToManaagerTimerIsNull()
		{
			new WorkerWrapper(new InvokeHandlerFake(), _nodeConfigurationFake, null, null, null, null, null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenPingToManagerTimerIsNull()
		{
			var postHttpRequestFake = new FakeHttpSender();

			new WorkerWrapper(new InvokeHandlerFake(),
				_nodeConfigurationFake,
				_nodeStartupNotification,
				null,
				null,
				null,
				null,
				new TrySendJobProgressToManagerTimerFake(_nodeConfigurationFake,
					postHttpRequestFake,
					5000));
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
											   _trySendJobProgressToManagerTimerFake);

			var actionResult = _workerWrapper.ValidateStartJob(new JobToDo());

			Assert.IsTrue(actionResult.IsBadRequest);
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
											   _trySendJobProgressToManagerTimerFake);

			var actionResult = _workerWrapper.ValidateStartJob(new JobToDo());

			Assert.IsTrue(actionResult.IsBadRequest);
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
											   _trySendJobProgressToManagerTimerFake);

			var actionResult = _workerWrapper.ValidateStartJob(null);

			Assert.IsTrue(actionResult.IsBadRequest);
		}
	}
}