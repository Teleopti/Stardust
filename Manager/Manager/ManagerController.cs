using System;
using System.Net.Http;
using System.Threading;
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
		private readonly JobManagerNewVersion _jobManager;
		private readonly INodeManager _nodeManager;

		private readonly Validator _validator;

		public ManagerController(INodeManager nodeManager,
		                         JobManagerNewVersion jobManager,
		                         Validator validator)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
			_validator = validator;
		}

		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
		{
			var isValidRequest = _validator.ValidateObject(jobQueueItem);

			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			_jobManager.AddItemToJobQueue(jobQueueItem);

			var msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
			                        WhoAmI(Request),
			                        jobQueueItem.JobId,
			                        jobQueueItem.Name);

			this.Log().InfoWithLineNumber(msg);

			Task.Factory.StartNew(() =>
			{
				_jobManager.AssignJobToWorkerNode();
			});

			return Ok(jobQueueItem.JobId);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelJobByJobId(Guid jobId)
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
		public IHttpActionResult GetAllJobs()
		{
			var allJobs = _jobManager.GetAllJobs();

			return Ok(allJobs);
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
		public IHttpActionResult GetJobByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			var job = _jobManager.GetJobByJobId(jobId);

			return Ok(job);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult GetJobDetailsByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			var jobDetailsByJobId = _jobManager.GetJobDetailsByJobId(jobId);

			return Ok(jobDetailsByJobId);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult RegisterHeartbeat([FromBody] Uri nodeUri)
		{
			var isValidRequest = _validator.ValidateUri(nodeUri);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");

				_jobManager.WorkerNodeRegisterHeartbeat(nodeUri.ToString());
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

				_jobManager.UpdateResultForJob(jobId,
				                              "Success",
											  DateTime.UtcNow);

				Thread.Sleep(TimeSpan.FromSeconds(2));

				_jobManager.AssignJobToWorkerNode();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailed jobFailed)
		{
			var isValidRequest = _validator.ValidateObject(jobFailed);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId ) : ( " +
				                               jobFailed.JobId + " )");

				var progress = new JobDetail
				{
					JobId = jobFailed.JobId,
					Created = DateTime.UtcNow,
					Detail = jobFailed.AggregateException.ToString()
				};

				_jobManager.CreateJobDetail(progress);

				_jobManager.UpdateResultForJob(jobFailed.JobId,
				                              "Failed",
											  DateTime.UtcNow);

				Thread.Sleep(TimeSpan.FromSeconds(2));

				_jobManager.AssignJobToWorkerNode();
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

				_jobManager.UpdateResultForJob(jobId,
				                              "Canceled",
											  DateTime.UtcNow);

				Thread.Sleep(TimeSpan.FromSeconds(2));

				_jobManager.AssignJobToWorkerNode();
			});

			return Ok();
		}


		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody] JobDetail model)
		{
			var isValidRequest = _validator.ValidateObject(model);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			Task.Factory.StartNew(() =>
			{
				_jobManager.CreateJobDetail(model);
			});

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

				_jobManager.AssignJobToWorkerNode();
			});

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IHttpActionResult Ping()
		{
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Nodes)]
		public IHttpActionResult GetAllWorkerNodes()
		{
			var workernodes = _jobManager.GetAllWorkerNodes();

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