using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
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
			                                                                    CallBackUriTemplateFake);

			_trySendJobProgressToManagerTimerFake =
				new TrySendJobProgressToManagerTimerFake(_nodeConfigurationFake,
														new FakeHttpSender(),
														1000);

			_pingToManagerFake = new PingToManagerFake();

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
			                                             CallBackUriTemplateFake,
														 _trySendJobProgressToManagerTimerFake);

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
			                                                     CallBackUriTemplateFake,
																 _trySendJobProgressToManagerTimerFake);

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
			                                                   CallBackUriTemplateFake,
															   _trySendJobProgressToManagerTimerFake);

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

			_nodeConfigurationFake = new NodeConfigurationFake(baseAddress,
			                                                   managerLocation,
			                                                   handlerAssembly,
			                                                   nodeName,
															   pingToManagerSeconds);

#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Logger.DebugWithLineNumber("Start TestFixtureTearDown.");
		}

		private Uri CallBackUriTemplateFake { get; set; }
		private NodeConfigurationFake _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private JobToDo _jobDefinition;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerWrapperTests));
		private PingToManagerFake _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobProgressToManagerTimerFake _trySendJobProgressToManagerTimerFake;

		[Test]
		public void ShouldBeAbleToCatchExceptionsFromJob() //faulting job
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new ThrowExceptionInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
											   new FakeHttpSender());

			var httpRequestMessage = new HttpRequestMessage();
			// Start job.
			var actionResult = _workerWrapper.StartJob(_jobDefinition,
			                                           httpRequestMessage);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.OK);
		}

		[Test]
		public void ShouldBeAbleToStartJob()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
											   new FakeHttpSender());

			var httpRequestMessage = new HttpRequestMessage();

			var actionResult = _workerWrapper.StartJob(_jobDefinition,
			                                           httpRequestMessage);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.OK);
		}

		[Test]
		public void ShouldBeAbleToTryCancelJob()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			var httpRequestMessage = new HttpRequestMessage();

			//-------------------------------------------
			// Start a job.
			//-------------------------------------------
			var actionResult = _workerWrapper.StartJob(_jobDefinition,
			                                           httpRequestMessage);
			//-------------------------------------------
			// Try cancel job.
			//-------------------------------------------
			_workerWrapper.CancelJob(_jobDefinition.Id);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.OK);
			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
		}

		[Test]
		public void ShouldNotThrowWhenCancellingAlreadyCancelledJob()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			var httpRequestMessage = new HttpRequestMessage();

			//-------------------------------------------
			// Start a job.
			//-------------------------------------------
			var actionResult = _workerWrapper.StartJob(_jobDefinition,
			                                           httpRequestMessage);
			//-------------------------------------------
			// Try cancel job.
			//-------------------------------------------
			_workerWrapper.CancelJob(_jobDefinition.Id);

			actionResult.ExecuteAsync(new CancellationToken());

			//-------------------------------------------
			// Try cancel job.
			//-------------------------------------------
			_workerWrapper.CancelJob(_jobDefinition.Id);

			Assert.IsTrue(_workerWrapper.IsCancellationRequested);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenInvokeHandlerIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(null,
			                                                 null,
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
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 _sendJobDoneTimer,
			                                                 null,
			                                                 null,
															 null,
			                                                 new FakeHttpSender());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJobFaultedTimerIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 _sendJobDoneTimer,
			                                                 _sendJobCanceledTimer,
			                                                 null,
															 null,
			                                                 new FakeHttpSender());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJodDoneTimerIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 null,
			                                                 null,
			                                                 null,
															 null,
			                                                 new FakeHttpSender());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeConfigurationIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
															 null,
			                                                 new FakeHttpSender());
		}


		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeStartupNotificationToManaagerTimerIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
															 null,
			                                                 new FakeHttpSender());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenPingToManagerTimerIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			var postHttpRequestFake = new FakeHttpSender();

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
															 new TrySendJobProgressToManagerTimerFake(_nodeConfigurationFake,
																									   postHttpRequestFake,
																									   5000), 
			                                                 postHttpRequestFake);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIdIsEmptyGuid()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
											   new FakeHttpSender());

			var actionResult = _workerWrapper.StartJob(new JobToDo(),
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode ==
			              HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsEmpty()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
											   new FakeHttpSender());

			var actionResult = _workerWrapper.StartJob(new JobToDo(),
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode ==
			              HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsNull()
		{
			Logger.DebugWithLineNumber("Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			var actionResult = _workerWrapper.StartJob(null,
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.BadRequest);
		}
	}
}