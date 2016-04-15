using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac.Extras.DynamicProxy2;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node.API
{
	[Intercept("log-calls")]
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
		public IHttpActionResult StartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			var isJobToDoValidObject = ValidateObject(jobQueueItemEntity);
			if (isJobToDoValidObject.IsBadRequest)
			{
				return BadRequest(isJobToDoValidObject.Message);
			}
			
			var isValidRequest = _workerWrapper.ValidateStartJob(jobQueueItemEntity);
			if (isValidRequest.IsBadRequest)
			{
				return BadRequest(isValidRequest.Message);
			}

			if (isValidRequest.IsConflict)
			{
				return Conflict();
			}

			Logger.InfoWithLineNumber("Received Start Job from Manager. JobId: " + jobQueueItemEntity.JobId);

			Task.Factory.StartNew(() =>
			{
				var startJobMessage = string.Format("{0} : Starting job ( jobId, jobName ) : ( {1}, {2} )",
				                                    _workerWrapper.WhoamI,
				                                    jobQueueItemEntity.JobId,
				                                    jobQueueItemEntity.Name);

				Logger.DebugWithLineNumber(startJobMessage);

				_workerWrapper.StartJob(jobQueueItemEntity);
			});

			return Ok();
		}

		private ObjectValidationResult ValidateObject(IValidatableObject validatableObject)
		{
			if (validatableObject != null)
			{
				var validationResults = 
					validatableObject.Validate(new ValidationContext(this));

				var enumerable = 
					validationResults as IList<ValidationResult> ?? validationResults.ToList();

				if (enumerable.Any())
				{
					return new ObjectValidationResult {IsBadRequest = true, Message = enumerable.First().ErrorMessage};
				}				
			}

			return new ObjectValidationResult();
		}

		private ObjectValidationResult ValidateJobId(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new ObjectValidationResult {IsBadRequest = true, Message = JobIdIsInvalid};
			}

			return new ObjectValidationResult();
		}

		[HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
		public IHttpActionResult TryCancelJob(Guid jobId)
		{
			var isValidRequest = ValidateJobId(jobId);
			if (isValidRequest.IsBadRequest)
			{
				return BadRequest(isValidRequest.Message);
			}

			Logger.InfoWithLineNumber(_workerWrapper.WhoamI +
			                          " : Received TryCancel request. jobId: " +
			                          jobId);

			Logger.DebugWithLineNumber(_workerWrapper.WhoamI + ": Try cancel job ( jobId ) : ( " + jobId + " )");

			var currentJob = _workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null || currentJob.JobId != jobId)
			{
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

			Logger.WarningWithLineNumber(_workerWrapper.WhoamI +
			                             ": Could not cancel job since job not found on this node. Manager sent job ( jobId ) : ( " +
			                             jobId + " )");
			return NotFound();
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsAlive)]
		public IHttpActionResult IsAlive()
		{
			return Ok();
		}
	}
}