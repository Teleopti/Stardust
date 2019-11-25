using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using NodeTest.Fakes;
using NodeTest.Fakes.InvokeHandlers;
using NodeTest.Fakes.Timers;
using NodeTest.JobHandlers;
using NUnit.Framework;
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

			_nodeStartupNotification = new NodeStartupNotificationToManagerFake(_nodeConfigurationFake,
			                                                                    new FakeHttpSender());
			_pingToManagerFake = new PingToManagerFake();

			_jobDetailSender = new JobDetailSender(new FakeHttpSender());

			_trySendJobDetailToManagerTimer =
				new TrySendJobDetailToManagerTimer(_jobDetailSender);
            _trySendJobDetailToManagerTimer.SetupAndStart(_nodeConfigurationFake);

            _sendJobDoneTimer = new SendJobDoneTimerFake(_jobDetailSender,
			                                             new FakeHttpSender());

			_sendJobCanceledTimer = new SendJobCanceledTimerFake(_jobDetailSender,
			                                                     new FakeHttpSender());

			_sendJobFaultedTimer = new SendJobFaultedTimerFake(_jobDetailSender,
			                                                   new FakeHttpSender());
			_now = new MutableNow();
		}

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_nodeConfigurationFake = new NodeConfiguration(
                new Uri("http://localhost:9001/StardustDashboard/"),
                Assembly.Load(typeof(WorkerModule).Assembly.FullName),
				14100,
				"TestNode",
				60,
				2000,true);

            _workerWrapperService = new WorkerWrapperServiceFake();
		}

        private WorkerWrapperServiceFake _workerWrapperService;
        private NodeConfiguration _nodeConfigurationFake;
		private IWorkerWrapper _workerWrapper;
		private NodeController _nodeController;
		private JobQueueItemEntity _jobQueueItemEntity;
		private IPingToManagerTimer _pingToManagerFake;
		private NodeStartupNotificationToManagerFake _nodeStartupNotification;
		private SendJobDoneTimerFake _sendJobDoneTimer;
		private SendJobCanceledTimerFake _sendJobCanceledTimer;
		private SendJobFaultedTimerFake _sendJobFaultedTimer;
		private TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
		private JobDetailSender _jobDetailSender;
		private MutableNow _now;

		[Test]
		public void CancelJobShouldReturnNotFoundWhenNodeIsIdle()
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
            _workerWrapperService.WorkerWrapper = _workerWrapper;

			_nodeController = new NodeController(_workerWrapperService)
			{
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                }
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
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, 
												_now);

            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
			{
				Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                }
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
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                }
            };

            _nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId);

			while (!_workerWrapper.IsWorking)
			{
				Thread.Sleep(200);
			}
			var actionResult = _nodeController.TryCancelJob(_jobQueueItemEntity.JobId);
			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken()).Result.StatusCode == HttpStatusCode.OK);
		}

		[Test]
		public void ShouldReturnBadRequestWhenCancelJobWithNoId()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);

            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
			{
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
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
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
			{
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
				Configuration = new HttpConfiguration()
			};


			var actionResult = _nodeController.PrepareToStartJob(null);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.BadRequest);
		}


		[Test]
		public void PrepareToStartJobShouldReturnIsWorkingWhenAlreadyProcessingJob()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);

            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
                Configuration = new HttpConfiguration()
			};

			var parameters = new TestJobParams("Test Job",
			                                  1);
			var ser = JsonConvert.SerializeObject(parameters);

			var jobToDo2 = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "Another name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			var actionResult1 = _nodeController.PrepareToStartJob(_jobQueueItemEntity);
			var response1 = actionResult1.ExecuteAsync(new CancellationToken()).Result;
			response1.StatusCode.Should().Be(HttpStatusCode.OK);
			var prepareToStartJobResult = JsonConvert.DeserializeObject<PrepareToStartJobResult>(response1.Content.ReadAsStringAsync().Result);
			prepareToStartJobResult.IsAvailable.Should().Be.True();
			
			var actionResult2 = _nodeController.PrepareToStartJob(jobToDo2);
			var response2 = actionResult2.ExecuteAsync(new CancellationToken()).Result;
			response2.StatusCode.Should().Be(HttpStatusCode.OK);
			var prepareToStartJobResult2 = JsonConvert.DeserializeObject<PrepareToStartJobResult>(response2.Content.ReadAsStringAsync().Result);
			prepareToStartJobResult2.IsAvailable.Should().Be.False();
		}

		[Test]
		public void PrepareToStartJobShouldReturnOkIfNotRunningJobAlready()
		{
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);

            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                }
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
			                                   _nodeStartupNotification,
			                                   _pingToManagerFake,
			                                   _sendJobDoneTimer,
			                                   _sendJobCanceledTimer,
			                                   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);

            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
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
											   _nodeStartupNotification,
											   _pingToManagerFake,
											   _sendJobDoneTimer,
											   _sendJobCanceledTimer,
											   _sendJobFaultedTimer,
											   _trySendJobDetailToManagerTimer,
											   _jobDetailSender, _now);
            _workerWrapper.Init(_nodeConfigurationFake);

            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                }
            };

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			var actionResult = _nodeController.StartJob(_jobQueueItemEntity.JobId);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
							  .Result.StatusCode ==
						  HttpStatusCode.OK);
		}

		[Test]
		public void ShouldTimeoutIfWaitingMoreThan5Minutes()
		{
			
			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
				_nodeStartupNotification,
				_pingToManagerFake,
				_sendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, _now);

            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
				Configuration = new HttpConfiguration()
			};

			var parameters = new TestJobParams("Test Job",1);
			var ser = JsonConvert.SerializeObject(parameters);

			var jobToDo2 = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "Another name",
				Serialized = ser,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_now.Is(new DateTime(2019,2,28,10,10,0));
			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();

			_now.Is(_now.UtcDateTime().AddMinutes(6));
			var actionResult = _nodeController.PrepareToStartJob(jobToDo2);

			Assert.IsTrue(actionResult.ExecuteAsync(new CancellationToken())
				.Result.IsSuccessStatusCode);
			_workerWrapper.IsWorking.Should().Be.False();
		}

		[Test]
		public void ShouldNotTimeoutIfWaitingLessThan5Minutes()
		{

			_workerWrapper = new WorkerWrapper(new ShortRunningInvokeHandlerFake(),
				_nodeStartupNotification,
				_pingToManagerFake,
				_sendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, _now);

            _workerWrapper.Init(_nodeConfigurationFake);
            _workerWrapperService.WorkerWrapper = _workerWrapper;

            _nodeController = new NodeController(_workerWrapperService)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("HTTP://localhost:14100")
                },
				Configuration = new HttpConfiguration()
			};

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
			_nodeController.PrepareToStartJob(_jobQueueItemEntity);

			_now.Is(_now.UtcDateTime().AddMinutes(3));
			var response = _nodeController.PrepareToStartJob(jobToDo2).ExecuteAsync(new CancellationToken()).Result;
			response.IsSuccessStatusCode.Should().Be.True();
			JsonConvert.DeserializeObject<PrepareToStartJobResult>(response.Content.ReadAsStringAsync().Result).IsAvailable.Should().Be.False();
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldNotTimeoutIfJobIsWorking()
		{
			_workerWrapper = new WorkerWrapper(new LongRunningInvokeHandlerFake(), 
				_nodeStartupNotification,
				_pingToManagerFake,
				_sendJobDoneTimer,
				_sendJobCanceledTimer,
				_sendJobFaultedTimer,
				_trySendJobDetailToManagerTimer,
				_jobDetailSender, _now);

			_workerWrapper.Init(_nodeConfigurationFake);
			_workerWrapperService.WorkerWrapper = _workerWrapper;

			_nodeController = new NodeController(_workerWrapperService)
			{
				Request = new HttpRequestMessage
				{
					RequestUri = new Uri("HTTP://localhost:14100")
				},
				Configuration = new HttpConfiguration()
			};

			var serializedParameters = JsonConvert.SerializeObject(new TestJobParams("Test Job", 1));
			var jobToDo2 = new JobQueueItemEntity
			{
				JobId = Guid.NewGuid(),
				Name = "Another name",
				Serialized = serializedParameters,
				Type = "NodeTest.JobHandlers.TestJobParams"
			};

			_now.Is(new DateTime(2019, 2, 28, 10, 10, 0));
			var responseMessage = _nodeController.PrepareToStartJob(_jobQueueItemEntity).ExecuteAsync(CancellationToken.None).Result;
			responseMessage.IsSuccessStatusCode.Should().Be.True();
			var prepareResultOk = JsonConvert.DeserializeObject<PrepareToStartJobResult>(responseMessage.Content.ReadAsStringAsync().Result);
			prepareResultOk.IsAvailable.Should().Be.True();
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			_workerWrapper.IsWorking.Should().Be.False();
			
			var startJobResult = _nodeController.StartJob(_jobQueueItemEntity.JobId)
				.ExecuteAsync(CancellationToken.None).Result;
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			startJobResult.IsSuccessStatusCode.Should().Be.True();			
			
			while(_workerWrapper.Task == null || _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_workerWrapper.IsWorking.Should().Be.True();
			_now.Is(_now.UtcDateTime().AddMinutes(6));
			_workerWrapper.IsWorking.Should().Be.True();
			
			var responseMessage2 = _nodeController.PrepareToStartJob(jobToDo2).ExecuteAsync(CancellationToken.None).Result;
			responseMessage2.IsSuccessStatusCode.Should().Be.True();
			var prepareResult2Ok = JsonConvert.DeserializeObject<PrepareToStartJobResult>(responseMessage2.Content.ReadAsStringAsync().Result);
			prepareResult2Ok.IsAvailable.Should().Be.False();
			
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
		}
	}
}