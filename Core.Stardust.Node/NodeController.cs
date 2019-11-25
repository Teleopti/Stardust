using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node
{

    [ApiController]
	public class NodeController : ControllerBase, IamStardustNodeController
	{
		private readonly WorkerWrapperService _workerWrapperService;
		private const string JobIdIsInvalid = "Job Id is invalid."; 
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

		public NodeController(WorkerWrapperService workerWrapperService)
		{
			_workerWrapperService = workerWrapperService;
		}

		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public IActionResult PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort((int)Request.Host.Port);

			var result = workerWrapper.ValidateStartJob(jobQueueItemEntity);
			var prepareToStartJobResult = new PrepareToStartJobResult {IsAvailable = !result.IsWorking && result.HttpResponseMessage.IsSuccessStatusCode};
			result.HttpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(prepareToStartJobResult));
			return Ok(result.HttpResponseMessage);
		}

		[HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
		public IActionResult StartJob(Guid jobId)
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort((int)Request.Host.Port);

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
		public IActionResult TryCancelJob(Guid jobId)
		{
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort((int)Request.Host.Port);

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
		public IActionResult IsAlive()
		{
			return Ok();
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsWorking)]
		public IActionResult IsWorking()
		{
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort((int)Request.Host.Port);

			return Ok(workerWrapper.IsWorking);
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
		public IActionResult IsIdle()
		{
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort((int)Request.Host.Port);

			var currentJob = workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null)
			{
				return Ok();
			}

			return Conflict();
		}
	}
}