using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Manager.Constants;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private readonly JobManager _jobManager;
		private readonly Validator _validator;

		private readonly INodeManager _nodeManager;

		public ManagerController(INodeManager nodeManager, JobManager jobManager, Validator validator)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
			_validator = validator;
		}

		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
		{
			var isValidRequest = _validator.ValidateObject(job);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			var jobReceived = new JobDefinition
			{
				Name = job.Name,
				Serialized = job.Serialized,
				Type = job.Type,
				UserName = job.UserName,
				Id = Guid.NewGuid()
			};

			_jobManager.AddJobDefinition(jobReceived);

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
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			_jobManager.CancelJobByJobId(jobId);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistoryList)]
		public IHttpActionResult JobHistoryList()
		{
			var jobHistory = _jobManager.GetAllJobHistories();

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
		public IHttpActionResult JobHistory(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			var jobHistory = _jobManager.GetJobHistoryByJobId(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			var jobHistoryDetail = _jobManager.GetJobHistoryDetailsByJobId(jobId);

			return Ok(jobHistoryDetail);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult Heartbeat([FromBody] Uri nodeUri)
		{
			var isValidRequest = _validator.ValidateUri(nodeUri);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");

				_jobManager.RegisterHeartbeat(nodeUri.ToString());
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Success");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailedModel jobFailedModel)
		{
			var isValidRequest = _validator.ValidateObject(jobFailedModel);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId ) : ( " +
				                               jobFailedModel.JobId + " )");

				var progress = new JobProgressModel
				{
					JobId = jobFailedModel.JobId,
					Created = DateTime.UtcNow,
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
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received cancel from a Node ( jobId ) : ( " + jobId + " )");

				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Canceled");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}


		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody] JobProgressModel model)
		{
			var isValidRequest = _validator.ValidateObject(model);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() => { _jobManager.ReportProgress(model); });

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
		{
			var isValidRequest = _validator.ValidateUri(nodeUri);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received init from Node. Node Uri : ( " + nodeUri + " )");

				_nodeManager.FreeJobIfAssingedToNode(nodeUri);
				_nodeManager.AddIfNeeded(nodeUri);
				_jobManager.CheckAndAssignNextJob();
			});

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

		private string WhoAmI(HttpRequestMessage request)
		{
			if (request == null)
			{
				return "[MANAGER, " + Environment.MachineName.ToUpper() + "]";
			}

			var baseUrl =
				request.RequestUri.GetLeftPart(UriPartial.Authority);

			return "[MANAGER, " + baseUrl + ", " + Environment.MachineName.ToUpper() + "]";
		}
	}
}