using System;
using System.Net;
using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//using log4net;
using Stardust.Core.Node.Extensions;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using WorkerWrapperService = Stardust.Core.Node.Workers.WorkerWrapperService;

namespace Stardust.Node
{
	[ApiController]
	public class NodeController : ControllerBase
	{
		private readonly WorkerWrapperService _workerWrapperService;
		private const string JobIdIsInvalid = "Job Id is invalid."; 
		private static readonly ILogger Logger = new LoggerFactory().CreateLogger(typeof (NodeController));

		public NodeController(WorkerWrapperService workerWrapperService)
		{
			_workerWrapperService = workerWrapperService;
		}

		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public ActionResult<PrepareToStartJobResult> PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(Request.Host.Port.GetValueOrDefault());

			var result = workerWrapper.ValidateStartJob(jobQueueItemEntity);
			var prepareToStartJobResult = new PrepareToStartJobResult {IsAvailable = !result.IsWorking && result.HttpResponseMessage.IsSuccessStatusCode};

			switch (result.HttpResponseMessage.StatusCode)
			{
				case HttpStatusCode.OK:
					return Ok(prepareToStartJobResult);
				case HttpStatusCode.BadRequest:
					return BadRequest(prepareToStartJobResult);
				default:
					throw new InvalidOperationException($"Unhandled HttpStatusCode {result.HttpResponseMessage.StatusCode}");	
			}
			
			//result.HttpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(prepareToStartJobResult));
			//return ResponseMessage(result.HttpResponseMessage);
		}

		[HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
		public ActionResult<string> StartJob(Guid jobId)
		{
			var workerWrapper =_workerWrapperService.GetWorkerWrapperByPort(Request.Host.Port.GetValueOrDefault());

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
		public ActionResult<string> TryCancelJob(Guid jobId)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(Request.Host.Port.GetValueOrDefault());

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
		public ActionResult IsAlive()
		{
			return Ok();
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsWorking)]
		public ActionResult<bool> IsWorking()
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(Request.Host.Port.GetValueOrDefault());

			return Ok(workerWrapper.IsWorking);
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
		public ActionResult IsIdle()
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(Request.Host.Port.GetValueOrDefault());

			var currentJob = workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null)
			{
				return Ok();
			}

			return Conflict();
		}
	}
}