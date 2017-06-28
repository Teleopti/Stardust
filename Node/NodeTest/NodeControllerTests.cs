using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
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
	public class NodeControllerTests
	{
		[SetUp]
		public void SetUp()
		{
			var parameters = new TestJobParams("Test Job",
			                                   TimeSpan.FromSeconds(1));

			var ser = JsonConvert.SerializeObject(parameters);

			_jobQueueItemEntity = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "JobToDo Name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
			                                                                    new FakeHttpSender());
			_pingToManagerFake = new PingToManagerFake();

			_jobDetailSender = new JobDetailSender(_nodeConfigurationFake, new FakeHttpSender());

			_trySendJobDetailToManagerTimer =
				new TrySendJobDetailToManagerTimer(_nodeConfigurationFake,
														 _jobDetailSender);

			_sendJobDoneTimer = new SendJobDoneTimerFake(_nodeConfigurationFake,
														 _jobDetailSender,
			                                             new FakeHttpSender());

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_nodeConfigurationFake,
																 _jobDetailSender,
			                                                     new FakeHttpSender());

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_nodeConfigurationFake,
															   _jobDetailSender,
			                                                   new FakeHttpSender());
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_nodeConfigurationFake = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				14100,
				"TestNode",
				60,
				2000);
		}

		private NodeConfiguration _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private NodeController _nodeController;
		private JobQueueItemEntity _jobQueueItemEntity;
		private PingToManagerFake _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
		private JobDetailSender _jobDetailSender;

		[Test]
		public void CancelJobShouldReturnNotFoundWhenNodeIsIdle()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage()
			};

			IHttpActionResult actionResultCancel = _nodeController.TryCancelJob(_jobQueueItemEntity.JobId);

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
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage()
			};
			
			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId);
			
			var wrongJobId = Guid.NewGuid();
			var actionResult = _nodeController.TryCancelJob(wrongJobId);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.NotFound);
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
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage()
			};

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId);

			var actionResult = _nodeController.TryCancelJob(_jobQueueItemEntity.JobId);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);
		}

		[Test]
		public void ShouldReturnBadRequestWhenCancelJobWithNoId()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			var actionResult = _nodeController.TryCancelJob(Guid.Empty);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}

		[Test]
		public void ShouldReturnBadRequestWhenPrepareToStartJobWithNoJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};


			var actionResult = _nodeController.PrepareToStartJob(null);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}


		[Test]
		public void PrepareToStartJobShouldReturnConflictWhenAlreadyProcessingJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
											   _nodeConfigurationFake,
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			var parameters = new TestJobParams("Test Job",
			                                   TimeSpan.FromSeconds(1));
			var ser = JsonConvert.SerializeObject(parameters);

			var jobToDo2 = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "Another name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);

			var actionResult = _nodeController.PrepareToStartJob(jobToDo2);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.Conflict);
		}

		[Test]
		public void PrepareToStartJobShouldReturnOkIfNotRunningJobAlready()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
											   _nodeConfigurationFake,
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage()
			};

			var actionResult = _nodeController.PrepareToStartJob(_jobQueueItemEntity);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);

		}


		[Test]
		public void StartJobShouldReturnBadRequestWhenStartJobIdDoesNotMatchPrepareJobId()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeConfigurationFake,
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpConfiguration()
			};

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);

			var actionResult = _nodeController.StartJob(Guid.NewGuid());

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}


		[Test]
		public void StartJobShouldReturnOkIfJobIdMatchPrepareJobId()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
											   _nodeConfigurationFake,
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender);

			_nodeController = new NodeController(_workerWrapper)
			{
				Request = new HttpRequestMessage()
			};

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			var actionResult = _nodeController.StartJob(_jobQueueItemEntity.JobId);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);
		}
		
	}
}