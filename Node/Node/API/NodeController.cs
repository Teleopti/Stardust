using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Autofac.Extras.DynamicProxy2;
using log4net;
using Stardust.Node.ActionResults;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;

namespace Stardust.Node.API
{
	[Intercept("log-calls")]
	public class NodeController : ApiController
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

		private readonly IWorkerWrapper _workerWrapper;

		public NodeController(IWorkerWrapper workerWrapper,
		                      INodeConfiguration nodeConfiguration)
		{
			_workerWrapper = workerWrapper;
			NodeConfiguration = nodeConfiguration;
		}

		private INodeConfiguration NodeConfiguration { get; set; }

		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public IHttpActionResult StartJob(JobToDo jobToDo)
		{
			var response = _workerWrapper.ValidateStartJob(jobToDo,
															Request);

			if (response.GetType() != typeof(OkResult))
			{
				return response;
			}

			Task.Factory.StartNew(() =>
			{
				response = _workerWrapper.StartJob(jobToDo,
												   Request);

				var startJobMessage = string.Format("{0} : Starting job ( jobId, jobName ) : ( {1}, {2} )",
													_workerWrapper.WhoamI,
													jobToDo.Id,
													jobToDo.Name);

				Logger.DebugWithLineNumber(startJobMessage);
			});

			return Ok();
		}

		[HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
		public IHttpActionResult TryCancelJob(Guid jobId)
		{
			Logger.InfoWithLineNumber(_workerWrapper.WhoamI + 
									 " : Received TryCancel request. jobId: " + 
									 jobId);

			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase("Job Id is invalid.");
			}

			Logger.DebugWithLineNumber(_workerWrapper.WhoamI + ": Try cancel job ( jobId ) : ( " + jobId + " )");

			var currentJob = _workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null || currentJob.Id != jobId)
			{
				return new NotFoundResultWithReasonPhrase("Job not found here.");
			}

			if (_workerWrapper.IsCancellationRequested)
			{
				return new ConflictResultWithReasonPhrase("Cancellation is already requested.");
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