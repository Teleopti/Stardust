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
		private readonly INodeManager _nodeManager;

		private readonly Validator _validator;

		public ManagerController(INodeManager nodeManager,
		                         JobManager jobManager,
		                         Validator validator)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
			_validator = validator;
		}

		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
		{
			//-----------------------------------------------
			// Assign job id to job queue item if not exists.
			//-----------------------------------------------
			if (jobQueueItem != null)
			{
				jobQueueItem.JobId = Guid.NewGuid();
			}

			//-----------------------------------------------
			// Validate job queue item.
			//-----------------------------------------------
			var isValidRequest = _validator.ValidateObject(jobQueueItem);

			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//-----------------------------------------------
			// Add to job queue.
			//-----------------------------------------------
			_jobManager.AddItemToJobQueue(jobQueueItem);

			var msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
			                        WhoAmI(Request),
			                        jobQueueItem.JobId,
			                        jobQueueItem.Name);

			this.Log().InfoWithLineNumber(msg);


			//-----------------------------------------------
			// Assign job queue item to a worker node.
			//-----------------------------------------------
			Task.Factory.StartNew(() =>
			{
				_jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri:null);
			});

			return Ok(jobQueueItem.JobId);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelJobByJobId(Guid jobId)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			//--------------------------------------
			// Cancel job.
			//--------------------------------------
			_jobManager.CancelJobByJobId(jobId);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Jobs)]
		public IHttpActionResult GetAllJobs()
		{
			var allJobs = _jobManager.GetAllJobs();

			return Ok(allJobs);
		}

		[HttpGet, Route(ManagerRouteConstants.JobByJobId)]
		public IHttpActionResult GetJobByJobId(Guid jobId)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Get job.
			//--------------------------------------
			var job = _jobManager.GetJobByJobId(jobId);

			return Ok(job);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetailByJobId)]
		public IHttpActionResult GetJobDetailsByJobId(Guid jobId)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Get job details..
			//--------------------------------------
			var jobDetailsByJobId = _jobManager.GetJobDetailsByJobId(jobId);

			return Ok(jobDetailsByJobId);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Register heartbeat from worker node.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + 
					": Received heartbeat from Node. Node Uri : ( " + workerNodeUri + " )");

				_nodeManager.WorkerNodeRegisterHeartbeat(workerNodeUri.ToString());
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobSucceed)]
		public IHttpActionResult JobSucceed(Guid jobId)
		{			
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Job done.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);

				this.Log().InfoWithLineNumber(WhoAmI(Request) + 
												": Received job done from a Node ( jobId, Node ) : ( " + jobId + ", " + workerNodeUri  + " )");

				_jobManager.UpdateResultForJob(jobId,
				                              "Success",
											  DateTime.UtcNow);

				_jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailed jobFailed)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateObject(jobFailed);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Job failed.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);

				this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId, Node ) : ( " +
				                               jobFailed.JobId + ", " + workerNodeUri + " )");

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
				
				_jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Job canceled.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);

				this.Log().InfoWithLineNumber(WhoAmI(Request) + 
					": Received cancel from a Node ( jobId, Node ) : ( " + jobId + ", " + workerNodeUri +")");

				_jobManager.UpdateResultForJob(jobId,
				                              "Canceled",
											  DateTime.UtcNow);
				
				_jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
			});

			return Ok();
		}


		[HttpPost, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult AddJobDetail([FromBody] JobDetail jobDetail)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateObject(jobDetail);
			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Add job detail.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				_jobManager.CreateJobDetail(jobDetail);
			});

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri workerNodeUri)
		{
			//--------------------------------------
			// Validate argument.
			//--------------------------------------
			var isValidRequest = _validator.ValidateUri(workerNodeUri);

			if (!isValidRequest.Success)
			{
				return BadRequest(isValidRequest.Message);
			}

			//--------------------------------------
			// Node initialize.
			//--------------------------------------
			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received init from Node. Node Uri : ( " + workerNodeUri + " )");

				_nodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(workerNodeUri,
																		 keepJobDetailsIfExists: true);

				_nodeManager.AddWorkerNode(workerNodeUri);

				_jobManager.AssignJobToWorkerNode(useThisWorkerNodeUri: null);
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