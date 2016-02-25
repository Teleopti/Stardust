using System;
using System.Threading.Tasks;
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
		private static readonly ILog Logger = LogManager.GetLogger(typeof (ManagerController));
		private readonly JobManager _jobManager;

		private readonly INodeManager _nodeManager;

		public ManagerController(INodeManager nodeManager,
		                         JobManager jobManager)
		{
			WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";

			_nodeManager = nodeManager;
			_jobManager = jobManager;
		}

		public string WhoAmI { get; private set; }

		[HttpPost, Route(ManagerRouteConstants.Job)]
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

			var jobReceived = new JobDefinition
			{
				Name = job.Name,
				Serialized = job.Serialized,
				Type = job.Type,
				UserName = job.UserName,
				Id = Guid.NewGuid()
			};
			_jobManager.Add(jobReceived);

			var msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
			                        WhoAmI,
			                        jobReceived.Id,
			                        jobReceived.Name);

			LogHelper.LogInfoWithLineNumber(Logger,
			                                msg);

			return Ok(jobReceived.Id);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelThisJob(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			_jobManager.CancelThisJob(jobId);
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistoryList)]
		public IHttpActionResult JobHistoryList()
		{
			return Ok(_jobManager.GetJobHistoryList());
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
		public IHttpActionResult JobHistory(Guid jobId)
		{
			var jobHistory = _jobManager.GetJobHistory(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			return Ok(_jobManager.JobHistoryDetails(jobId));
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public void Heartbeat([FromBody] Uri nodeUri)
		{
			Task.Factory.StartNew(() =>
			{
				_jobManager.RegisterHeartbeat(nodeUri);
				_jobManager.CheckAndAssignNextJob();
			});
			
			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

			_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
			                                         "Success");

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
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
		{
			_nodeManager.FreeJobIfAssingedToNode(nodeUri);
			_nodeManager.AddIfNeeded(nodeUri);

			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI + ": Received init from Node. Node Uri : ( " + nodeUri + " )");

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IHttpActionResult Ping()
		{
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Nodes)]
		public IHttpActionResult Nodes()
		{
			return Ok(_jobManager.Nodes());
		}

	}
}