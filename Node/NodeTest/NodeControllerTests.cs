using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
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

		[OneTimeSetUpAttribute]
		public void TestFixtureSetUp()
		{
			_nodeConfigurationFake = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

			_nodeController.TryCancelJob(_jobQueueItemEntity.JobId).Should().Be.OfType<NotFoundResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };
			
			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId);
			
			var wrongJobId = Guid.NewGuid();
			_nodeController.TryCancelJob(wrongJobId).Should().Be.OfType<NotFoundResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

            _nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId);

			while (!_workerWrapper.IsWorking)
			{
				Thread.Sleep(200);
			}
			_nodeController.TryCancelJob(_jobQueueItemEntity.JobId).Should().Be.OfType<OkResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

			_nodeController.TryCancelJob(Guid.Empty).Should().Be.OfType<BadRequestObjectResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

            var result = (ContentResult)_nodeController.PrepareToStartJob(null);
            result.StatusCode.Should().Be.EqualTo(HttpStatusCode.BadRequest);
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
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

			var actionResult1 = (ContentResult)_nodeController.PrepareToStartJob(_jobQueueItemEntity);
            actionResult1.StatusCode.Should().Be(HttpStatusCode.OK);
			var prepareToStartJobResult = JsonConvert.DeserializeObject<PrepareToStartJobResult>(actionResult1.Content);
			prepareToStartJobResult.IsAvailable.Should().Be.True();
			
			var actionResult2 = (ContentResult)_nodeController.PrepareToStartJob(jobToDo2);
			actionResult2.StatusCode.Should().Be(HttpStatusCode.OK);
			var prepareToStartJobResult2 = JsonConvert.DeserializeObject<PrepareToStartJobResult>(actionResult2.Content);
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

            var result = (ContentResult)_nodeController.PrepareToStartJob(_jobQueueItemEntity);
            result.StatusCode.Should().Be.EqualTo(HttpStatusCode.OK);
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);

			_nodeController.StartJob(Guid.NewGuid()).Should().Be.OfType<BadRequestObjectResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
            };

			_nodeController.PrepareToStartJob(_jobQueueItemEntity);
			_nodeController.StartJob(_jobQueueItemEntity.JobId).Should().Be.OfType<OkResult>();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
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

            var result = (ContentResult)_nodeController.PrepareToStartJob(jobToDo2);
            result.StatusCode.Should().Be.EqualTo(HttpStatusCode.OK);
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
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
			var response = (ContentResult)_nodeController.PrepareToStartJob(jobToDo2);
            response.StatusCode.Should().Be.EqualTo(HttpStatusCode.OK);
			JsonConvert.DeserializeObject<PrepareToStartJobResult>(response.Content).IsAvailable.Should().Be.False();
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
                ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext() { Request = { Host = new HostString("HTTP://localhost:14100") } }, new RouteData(), new ControllerActionDescriptor()))
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
			var responseMessage = (ContentResult)_nodeController.PrepareToStartJob(_jobQueueItemEntity);
            responseMessage.StatusCode.Should().Be.EqualTo(HttpStatusCode.OK);
            var prepareResultOk = JsonConvert.DeserializeObject<PrepareToStartJobResult>(responseMessage.Content);
			prepareResultOk.IsAvailable.Should().Be.True();
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			_workerWrapper.IsWorking.Should().Be.False();
			
			var startJobResult = _nodeController.StartJob(_jobQueueItemEntity.JobId).Should().Be.OfType<OkResult>();
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
			
			while(_workerWrapper.Task == null || _workerWrapper.Task.Status != TaskStatus.Running) { Thread.Sleep(100); }
			_workerWrapper.IsWorking.Should().Be.True();
			_now.Is(_now.UtcDateTime().AddMinutes(6));
			_workerWrapper.IsWorking.Should().Be.True();
			
			var responseMessage2 = (ContentResult)_nodeController.PrepareToStartJob(jobToDo2);
            responseMessage2.StatusCode.Should().Be.EqualTo(HttpStatusCode.OK);
			var prepareResult2Ok = JsonConvert.DeserializeObject<PrepareToStartJobResult>(responseMessage2.Content);
			prepareResult2Ok.IsAvailable.Should().Be.False();
			
			_workerWrapper.GetCurrentMessageToProcess().Should().Not.Be.Null();
		}
	}
}