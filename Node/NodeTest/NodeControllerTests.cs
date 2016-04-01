using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Stardust.Node.Workers;

namespace NodeTest
{
	[TestFixture]
	public class NodeControllerTests
	{
		[SetUp]
		public void SetUp()
		{
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

			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
			                                                                    _callBackTemplateUriFake,
			                                                                    new FakeHttpSender());
			_pingToManagerFake = new PingToManagerFake();

			_trySendJobProgressToManagerTimerFake =
				new TrySendJobProgressToManagerTimerFake(_nodeConfigurationFake,
				                                         new FakeHttpSender(),
				                                         1000);

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
			                                             _callBackTemplateUriFake,
			                                             _trySendJobProgressToManagerTimerFake,
			                                             new FakeHttpSender());

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
			                                                     _callBackTemplateUriFake,
			                                                     _trySendJobProgressToManagerTimerFake,
			                                                     new FakeHttpSender());

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
			                                                   _callBackTemplateUriFake,
			                                                   _trySendJobProgressToManagerTimerFake,
			                                                   new FakeHttpSender());
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			var baseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

			var managerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);

			var handlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);

			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			var pingToManagerSeconds =
				Convert.ToDouble(ConfigurationManager.AppSettings["PingToManagerSeconds"]);

			_nodeConfigurationFake = new NodeConfigurationFake(baseAddress,
			                                                   managerLocation,
			                                                   handlerAssembly,
			                                                   nodeName,
			                                                   pingToManagerSeconds);


			_callBackTemplateUriFake = managerLocation;
#if DEBUG
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));
#endif
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Logger.DebugWithLineNumber("Start TestFixtureTearDown");
		}

		private NodeConfigurationFake _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private NodeController _nodeController;
		private JobToDo _jobToDo;
		private Uri _callBackTemplateUriFake;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeControllerTests));
		private PingToManagerFake _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobProgressToManagerTimerFake _trySendJobProgressToManagerTimerFake;

		[Test]
		public void CancelJobShouldReturnNotFoundWhenCancellingJobWhenIdle()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			IHttpActionResult actionResultCancel = _nodeController.TryCancelJob(_jobToDo.Id);

			Assert.IsTrue(actionResultCancel.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.NotFound);

		}

		[Test]
		public void CancelJobShouldReturnNotFoundWhenCancellingWrongJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			var wrongJobToDo = new JobToDo
			{
				Id = Guid.NewGuid(),
				Name = "Another name",
				Type = "NodeTest.JobHandlers.TestJobParams",
				Serialized = "Serialized data"
			};

			var actionResult = _nodeController.StartJob(_jobToDo);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);

			actionResult = _nodeController.TryCancelJob(wrongJobToDo.Id);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.NotFound);

			_sendJobDoneTimer.Wait.Wait(TimeSpan.FromSeconds(3)); // let job finish
		}

		[Test]
		public void CancelJobShouldReturnOkWhenSuccessful()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			_nodeController.StartJob(_jobToDo);

			var actionResult = _nodeController.TryCancelJob(_jobToDo.Id);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);

			_sendJobDoneTimer.Wait.Wait(TimeSpan.FromSeconds(15)); // let job finish
		}

		[Test]
		public void ShouldReturnBadRequestWhenJobDefinitionIsNullCancelJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			var actionResult = _nodeController.TryCancelJob(Guid.Empty);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestWhenJobDefinitionIsNullStartJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};


			var actionResult = _nodeController.StartJob(null);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}

		[Test]
		public void StartJobShouldReturnConflictWhenAlreadyProcessingJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			var parameters = new TestJobParams("hejhopp",
			                                   "i lingonskogen");
			var ser = JsonConvert.SerializeObject(parameters);

			var jobToDo2 = new JobToDo
			{
				Id = Guid.NewGuid(),
				Name = "Another name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_nodeController.StartJob(_jobToDo);

			var actionResult = _nodeController.StartJob(jobToDo2);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.Conflict);

			_sendJobDoneTimer.Wait.Wait(TimeSpan.FromSeconds(3)); // let job finish
		}

		[Test]
		public void StartJobShouldReturnOkIfNotRunningJobAlready()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
			                                   _trySendJobProgressToManagerTimerFake,
			                                   new FakeHttpSender());

			_nodeController = new NodeController(_workerWrapper, _nodeConfigurationFake)
			{
				Request = new HttpRequestMessage()
			};

			var actionResult = _nodeController.StartJob(_jobToDo);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);

			_sendJobDoneTimer.Wait.Wait(TimeSpan.FromSeconds(3)); // let job finish
		}
	}
}