﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node
{
	public class NodeController : ApiController
	{
		private readonly WorkerWrapperService _workerWrapperService;
		private const string JobIdIsInvalid = "Job Id is invalid."; 
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

		//private readonly IWorkerWrapper _workerWrapper;
		//private readonly NodeConfigurationService _nodeConfigurationService;

		//public NodeController(IWorkerWrapper workerWrapper, NodeConfigurationService nodeConfigurationService)
		public NodeController(WorkerWrapperService workerWrapperService)
		{
			_workerWrapperService = workerWrapperService;
			//_workerWrapper = workerWrapper;
			//_nodeConfigurationService = nodeConfigurationService;
		}

		//public void Init(NodeConfiguration nodeConfiguration)
		//{
		//	_workerWrapperService.GetWorkerWrapperByPort()
		//}

		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public IHttpActionResult PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			//_workerWrapper.Init(_nodeConfigurationService.GetConfigurationForPort(ActionContext.Request.RequestUri.Port));
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			var isValidRequest = workerWrapper.ValidateStartJob(jobQueueItemEntity);
			if (!isValidRequest.IsSuccessStatusCode)
			{
				return ResponseMessage(isValidRequest);
			}
			if (workerWrapper.IsWorking) return Conflict();

			return Ok();
		}

		[HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
		public IHttpActionResult StartJob(Guid jobId)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			if (jobId == Guid.Empty)
			{
				return BadRequest(JobIdIsInvalid);
			}

			workerWrapper.CancelTimeoutCurrentMessageTask();

			var currentMessage = workerWrapper.GetCurrentMessageToProcess();

			if (currentMessage == null)
			{
				return BadRequest("Current message has timed out.");
			}

			if (currentMessage.JobId != jobId)
			{
				return BadRequest("Current message job id does not match with job id argument.");
			}

			Task.Run(() =>
			{				
				var startJobMessage = $"{workerWrapper.WhoamI} : Starting job ( jobId, jobName ) : ( {currentMessage.JobId}, {currentMessage.Name} )";

				Logger.InfoWithLineNumber(startJobMessage);

				workerWrapper.StartJob(currentMessage);
			});

			return Ok();
		}

		[HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
		public IHttpActionResult TryCancelJob(Guid jobId)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			if (jobId == Guid.Empty)
			{
				return BadRequest(JobIdIsInvalid);
			}

			Logger.InfoWithLineNumber(workerWrapper.WhoamI +
			                          " : Received Cancel request. jobId: " + jobId);
			
			var currentJob = workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null || currentJob.JobId != jobId)
			{
				Logger.WarningWithLineNumber(workerWrapper.WhoamI +
							 ": Could not cancel job since job not found on this node. Manager sent job ( jobId ) : ( " +
							 jobId + " )");

				return NotFound();
			}

			if (workerWrapper.IsCancellationRequested)
			{
				return Conflict();
			}

			workerWrapper.CancelJob(jobId);

			if (workerWrapper.IsCancellationRequested)
			{
				return Ok();
			}

			return NotFound();
		}


		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsAlive)]
		public IHttpActionResult IsAlive()
		{
			return Ok();
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsWorking)]
		public IHttpActionResult IsWorking()
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			return Ok(workerWrapper.IsWorking);
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
		public IHttpActionResult IsIdle()
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			var currentJob = workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null)
			{
				return Ok();
			}

			return Conflict();
		}
	}
}