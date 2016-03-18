using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Manager.Constants;
using Stardust.Manager.Extensions;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private readonly JobManager _jobManager;

		private readonly INodeManager _nodeManager;

		public ManagerController(INodeManager nodeManager,
		                         JobManager jobManager)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;			
		}

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
			                        WhoAmI(Request),
			                        jobReceived.Id,
			                        jobReceived.Name);

			this.Log().InfoWithLineNumber(msg);

			Task.Factory.StartNew(() => { _jobManager.CheckAndAssignNextJob(); });

			return Ok(jobReceived.Id);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelThisJob(Guid jobId)
		{
			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			_jobManager.CancelThisJob(jobId);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistoryList)]
		public IHttpActionResult JobHistoryList()
		{
			var jobHistory = _jobManager.GetJobHistoryList();

			return Ok(jobHistory);
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
			var jobHistoryDetail = _jobManager.JobHistoryDetails(jobId);

			return Ok(jobHistoryDetail);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult Heartbeat([FromBody] Uri nodeUri)
		{
			if (nodeUri != null)
			{
				Task.Factory.StartNew(() =>
				{
					_jobManager.RegisterHeartbeat(nodeUri.ToString());
				});

				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");
				return Ok();
			}

			this.Log().WarningWithLineNumber(WhoAmI(Request) + ": Received heartbeat from Node with invalid uri.");
			return BadRequest();
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{
			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

			Task.Factory.StartNew(() =>
			{
				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Success");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailedModel jobFailedModel)
		{			
			this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId ) : ( " + jobFailedModel.JobId + " )");

			Task.Factory.StartNew(() =>
			{
				JobProgressModel progress = new JobProgressModel
				{
					JobId = jobFailedModel.JobId,
					Created = DateTime.Now,
					ProgressDetail = jobFailedModel.AggregateException.ToString()
				};

				_jobManager.ReportProgress(progress);

				_jobManager.SetEndResultOnJobAndRemoveIt(jobFailedModel.JobId,
				                                         "Failed");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobHasBeenCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{

			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received cancel from a Node ( jobId ) : ( " + jobId + " )");

			Task.Factory.StartNew(() =>
			{
				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Canceled");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody] JobProgressModel model)
		{

			if (model == null)
			{
				return BadRequest();
			}

			if (model.JobId == Guid.Empty || string.IsNullOrEmpty(model.ProgressDetail))
			{
				return BadRequest();
			}

			Task.Factory.StartNew(() =>
			{
				_jobManager.ReportProgress(model);
			});			

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
		{

			Task.Factory.StartNew(() =>
			{
				_nodeManager.FreeJobIfAssingedToNode(nodeUri);
				_nodeManager.AddIfNeeded(nodeUri);
				_jobManager.CheckAndAssignNextJob();
			});

			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received init from Node. Node Uri : ( " + nodeUri + " )");

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
			

			var workernodes = _jobManager.Nodes();

			return Ok(workernodes);
		}

		private string WhoAmI(HttpRequestMessage Request)
		{
			if (Request != null)
			{
				var baseUrl =
				Request.RequestUri.GetLeftPart(UriPartial.Authority);
				
				return "[MANAGER, " + baseUrl + ", " + Environment.MachineName.ToUpper() + "]";
			}
			return "[MANAGER, " + Environment.MachineName.ToUpper() + "]";
		}
	}
}