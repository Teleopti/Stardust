using System;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Manager.Constants;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private readonly INodeManager _nodeManager;
		private readonly JobManager _jobManager;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ManagerController));

		public string WhoAmI { get; private set; }

		public ManagerController(INodeManager nodeManager, JobManager jobManager)
		{
			WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";
			_nodeManager = nodeManager;
			_jobManager = jobManager;
		}

		[HttpPost, Route(ManagerRouteConstants.StartJob)]
		public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
		{
			if (job == null)
				return new BadRequestResult(Request);
			if (string.IsNullOrEmpty(job.Name) || string.IsNullOrEmpty(job.Serialized) || string.IsNullOrEmpty(job.Type) || string.IsNullOrEmpty(job.UserName))
				return new BadRequestResult(Request);

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
			Logger.Info(WhoAmI + ": New job received from client. JobId: " + jobReceived.Id);
			return Ok(jobReceived.Id);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public void Heartbeat([FromBody] Uri nodeUri)
		{
			Logger.Info(WhoAmI + ": Received heartbeat from Node : " + nodeUri);
			_jobManager.CheckAndAssignNextJob();
		}


		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public void CancelThisJob(Guid jobId)
		{
			Logger.Info(WhoAmI + ": Received cancel from client. JobId = " + jobId);

			_jobManager.CancelThisJob(jobId);
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{
			Logger.Info(WhoAmI + ": Received job done from a Node. JobId = " + jobId);
			_jobManager.SetEndResultOnJobAndRemoveIt(jobId, "Success");
			//should we do this here also or only on heartbeats
			_jobManager.CheckAndAssignNextJob();

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed(Guid jobId)
		{
			Logger.Error(WhoAmI + ": Received job failed from a Node. JobId = " + jobId);
			_jobManager.SetEndResultOnJobAndRemoveIt(jobId, "Failed");

			Logger.Info("Received cancel from client. JobId = " + jobId);
			_jobManager.CancelThisJob(jobId);
			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobHasBeenCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
			Logger.Info(WhoAmI + ": Received cancel from a Node. JobId = " + jobId);
			_jobManager.SetEndResultOnJobAndRemoveIt(jobId, "Canceled");

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody]JobProgressModel model)
		{
			_jobManager.ReportProgress(model);

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUrl)
		{
			Logger.Info(WhoAmI + ": Received init from node " + nodeUrl);
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
	}
}