using System;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Manager.Constants;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ManagerController));

		private readonly INodeManager _nodeManager;
		private readonly JobManager _jobManager;

		public string WhoAmI { get; private set; }

		//protected override void Dispose(bool disposing)
		//{
		//    LogHelper.LogInfoWithLineNumber(Logger,"Start disposing manager controller.");

		//    base.Dispose(disposing);

		//    LogHelper.LogInfoWithLineNumber(Logger, "Finished disposing manager controller.");
		//}

		public ManagerController(INodeManager nodeManager,
										 JobManager jobManager)
		{
			WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";

			_nodeManager = nodeManager;
			_jobManager = jobManager;
		}

		[HttpPost, Route(ManagerRouteConstants.StartJob)]
		public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
		{
			if (job == null)
			{
				return new BadRequestResult(Request);
			}

			if (string.IsNullOrEmpty(job.Name) ||
				 string.IsNullOrEmpty(job.Serialized) ||
				 string.IsNullOrEmpty(job.Type) ||
				 string.IsNullOrEmpty(job.UserName))
			{
				return new BadRequestResult(Request);
			}

			var jobReceived = new JobDefinition()
			{
				Name = job.Name,
				Serialized = job.Serialized,
				Type = job.Type,
				UserName = job.UserName,
				//Should we do it here or?
				Id = Guid.NewGuid()
			};
			_jobManager.Add(jobReceived);

			string msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
												WhoAmI,
												jobReceived.Id,
												jobReceived.Name);

			LogHelper.LogInfoWithLineNumber(Logger,
													  msg);

			return Ok(jobReceived.Id);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public void Heartbeat([FromBody] Uri nodeUri)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
													  WhoAmI + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");

			_jobManager.CheckAndAssignNextJob();
		}


		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public void CancelThisJob(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
													  WhoAmI + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			_jobManager.CancelThisJob(jobId);
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
													  WhoAmI + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

			_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
																  "Success");

			//should we do this here also or only on heartbeats
			_jobManager.CheckAndAssignNextJob();

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed(Guid jobId)
		{
			LogHelper.LogErrorWithLineNumber(Logger,
														WhoAmI + ": Received job failed from a Node ( jobId ) : ( " + jobId + " )");

			_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
																  "Failed");
			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobHasBeenCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
													  WhoAmI + ": Received cancel from a Node ( jobId ) : ( " + jobId + " )");

			_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
																  "Canceled");

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody] JobProgressModel model)
		{
			_jobManager.ReportProgress(model);

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUrl)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
													  WhoAmI + ": Received init from Node. Node Uri : ( " + nodeUrl + " )");

			_nodeManager.FreeJobIfAssingedToNode(nodeUrl);
			_nodeManager.AddIfNeeded(nodeUrl);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
		public IHttpActionResult JobHistory(Guid jobId)
		{
			JobHistory jobHistory = _jobManager.GetJobHistory(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.JobHistoryList)]
		public IHttpActionResult JobHistoryList()
		{
			return Ok(_jobManager.GetJobHistoryList());
		}

		[HttpPost, Route(ManagerRouteConstants.JobHistoryDetails)]
		public IHttpActionResult JobHistoryDetails([FromBody]Guid jobId)
		{
			return Ok(_jobManager.JobHistoryDetails(jobId));
		}
	}
}