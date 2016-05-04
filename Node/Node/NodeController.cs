﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node
{
	public class NodeController : ApiController
	{
		private const string JobIdIsInvalid = "Job Id is invalid.";
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

		private readonly IWorkerWrapper _workerWrapper;

		public NodeController(IWorkerWrapper workerWrapper,
		                      NodeConfiguration nodeConfiguration)
		{
			_workerWrapper = workerWrapper;
			NodeConfiguration = nodeConfiguration;
		}

		private NodeConfiguration NodeConfiguration { get; set; }
		
		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public IHttpActionResult PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			var isValidRequest = _workerWrapper.ValidateStartJob(jobQueueItemEntity);
			if (!isValidRequest.IsSuccessStatusCode)
			{
				return ResponseMessage(isValidRequest);
			}

			Task.Factory.StartNew(() =>
			{
				var task=_workerWrapper.CreateTimeoutCurrentMessageTask(jobQueueItemEntity);

				try
				{
					task.Start();
				}

				catch (Exception)
				{				
				}				
			});

			return Ok();
		}

		[HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
		public IHttpActionResult StartJob(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return BadRequest(JobIdIsInvalid);
			}

			_workerWrapper.CancelTimeoutCurrentMessageTask();

			var currentMessage = _workerWrapper.GetCurrentMessageToProcess();

			if (currentMessage == null)
			{
				return BadRequest("Current message has timed out.");
			}

			if (currentMessage.JobId != jobId)
			{
				return BadRequest("Current message job id does not match with job id argument.");
			}

			Task.Factory.StartNew(() =>
			{				
				var startJobMessage = string.Format("{0} : Starting job ( jobId, jobName ) : ( {1}, {2} )",
				                                    _workerWrapper.WhoamI,
													currentMessage.JobId,
													currentMessage.Name);

				Logger.InfoWithLineNumber(startJobMessage);

				_workerWrapper.StartJob(currentMessage);
			});

			return Ok();
		}

		[HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
		public IHttpActionResult TryCancelJob(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return BadRequest(JobIdIsInvalid);
			}

			Logger.InfoWithLineNumber(_workerWrapper.WhoamI +
			                          " : Received Cancel request. jobId: " + jobId);
			
			var currentJob = _workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null || currentJob.JobId != jobId)
			{
				Logger.WarningWithLineNumber(_workerWrapper.WhoamI +
							 ": Could not cancel job since job not found on this node. Manager sent job ( jobId ) : ( " +
							 jobId + " )");

				return NotFound();
			}

			if (_workerWrapper.IsCancellationRequested)
			{
				return Conflict();
			}

			_workerWrapper.CancelJob(jobId);

			if (_workerWrapper.IsCancellationRequested)
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


		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
		public IHttpActionResult IsIdle()
		{			
			var currentJob = _workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null)
			{
				return Ok();
			}

			return Conflict();
		}
	}
}