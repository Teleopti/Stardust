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

			_pingToManagerFake = new PingToManagerFake();

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
			                                             CallBackUriTemplateFake);

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
			                                                     CallBackUriTemplateFake);

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
			                                                   CallBackUriTemplateFake);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);
			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);
			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			var pingToManagerIdleDelay =
				Convert.ToDouble(ConfigurationManager.AppSettings["PingToManagerIdleDelay"]);

			var pingToManagerRunningDelay =
				Convert.ToDouble(ConfigurationManager.AppSettings["PingToManagerRunningDelay"]);

			CallBackUriTemplateFake = managerLocation;

			_nodeConfigurationFake = new NodeConfigurationFake(baseAddress,
			                                                   managerLocation,
			                                                   handlerAssembly,
			                                                   nodeName,
															   pingToManagerIdleDelay,
															   pingToManagerRunningDelay);

#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Thread.Sleep(TimeSpan.FromSeconds(5));
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start TestFixtureTearDown.");
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

		[Test]
		public void ShouldBeAbleToCatchExceptionsFromJob() //faulting job
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new ThrowExceptionInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

			var httpRequestMessage = new HttpRequestMessage();

			var actionResult = _workerWrapper.StartJob(_jobDefinition,
			                                           httpRequestMessage);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.OK);
		}

		[Test]
		public void ShouldBeAbleToTryCancelJob()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 _sendJobDoneTimer,
			                                                 null,
			                                                 null,
			                                                 new PostHttpRequestFake());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJobFaultedTimerIsNull()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 _sendJobDoneTimer,
			                                                 _sendJobCanceledTimer,
			                                                 null,
			                                                 new PostHttpRequestFake());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenJodDoneTimerIsNull()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 _pingToManagerFake,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 new PostHttpRequestFake());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowArgumentNullExceptionWhenNodeConfigurationIsNull()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
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
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			IWorkerWrapper workerWrapper = new WorkerWrapper(new InvokeHandlerFake(),
			                                                 _nodeConfigurationFake,
			                                                 _nodeStartupNotification,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 null,
			                                                 new PostHttpRequestFake());
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIdIsEmptyGuid()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

			var actionResult = _workerWrapper.StartJob(new JobToDo(),
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode ==
			              HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsEmpty()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

			var actionResult = _workerWrapper.StartJob(new JobToDo(),
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode ==
			              HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnBadRequestWhenMessageIsNull()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Starting test...");

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   new PostHttpRequestFake());

			var actionResult = _workerWrapper.StartJob(null,
			                                           new HttpRequestMessage());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				              .Result.StatusCode == HttpStatusCode.BadRequest);
		}
	}
}