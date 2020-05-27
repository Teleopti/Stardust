using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Manager.Constants;
using Stardust.Manager.Extensions;
using Stardust.Manager.Models;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private readonly IJobManager _jobManager;
		private readonly NodeManager _nodeManager;

		private readonly Validator _validator;

		public ManagerController(NodeManager nodeManager,
                                 IJobManager jobManager,
		                         Validator validator)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
			_validator = validator;
		}

		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
		{
			jobQueueItem.JobId = Guid.NewGuid();

			var isValidRequest = _validator.ValidateObject(jobQueueItem);
			if (!isValidRequest.Success)  return BadRequest(isValidRequest.Message);
			
			_jobManager.AddItemToJobQueue(jobQueueItem);

			var msg = $"{WhoAmI(Request)} : New job received from client ( jobId, jobName ) : ( {jobQueueItem.JobId}, {jobQueueItem.Name} )";
			this.Log().InfoWithLineNumber(msg);

			return Ok(jobQueueItem.JobId);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelJobByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");
			
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
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			var job = _jobManager.GetJobByJobId(jobId);

			return Ok(job);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetailByJobId)]
		public IHttpActionResult GetJobDetailsByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			var jobDetailsByJobId = _jobManager.GetJobDetailsByJobId(jobId);

			return Ok(jobDetailsByJobId);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
		{
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

		    Task.Run(() =>
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
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

            var task = Task.Run(() =>
			{
				var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);

				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job done from a Node ( jobId, Node ) : ( " + jobId + ", " + workerNodeUri  + " )");

				_jobManager.UpdateResultForJob(jobId,
				                              "Success",
				                              workerNodeUri,
											  DateTime.UtcNow);
			});

            try
            {
                task.Wait();
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }

            return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailed jobFailed)
		{
			var isValidRequest = _validator.ValidateObject(jobFailed);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

			var task = Task.Run(() =>
            {
                var workerNodeUri = Request.RequestUri?.GetLeftPart(UriPartial.Authority);

                this.Log().ErrorWithLineNumber(
                    $"{WhoAmI(Request)}: Received job failed from a Node ( jobId, Node ) : ( {jobFailed.JobId}, {workerNodeUri??"Unknown Uri"} )");

                var progress = new JobDetail
                {
                    JobId = jobFailed.JobId,
                    Created = DateTime.UtcNow,
                    Detail = jobFailed.AggregateException?.ToString()??"No Exception specified for job"
                };

                _jobManager.CreateJobDetail(progress, workerNodeUri);

                _jobManager.UpdateResultForJob(jobFailed.JobId,
                    "Failed",
                    workerNodeUri,
                    DateTime.UtcNow);
            });

            try
            {
                var taskIsSuccessful = task.Wait(TimeSpan.FromMinutes(1));
                if (!taskIsSuccessful)
                {
                    var aggregationException = new AggregateException(
                        new TimeoutException($"Timeout while executing {nameof(JobFailed)}"));
                    return InternalServerError(aggregationException);
                }
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }

            return Ok();
        }
		
		[HttpPost, Route(ManagerRouteConstants.JobCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			Task.Run(() =>
			{
				var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);

				this.Log().InfoWithLineNumber(WhoAmI(Request) + 
					": Received cancel from a Node ( jobId, Node ) : ( " + jobId + ", " + workerNodeUri +")");

				_jobManager.UpdateResultForJob(jobId,
				                              "Canceled",
				                              workerNodeUri,
											  DateTime.UtcNow);
			});

            return Ok();
        }


		[HttpPost, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult AddJobDetail([FromBody] IList<JobDetail> jobDetails)
		{
			var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
			foreach (var detail in jobDetails)
			{
				var isValidRequest = _validator.ValidateObject(detail);
				if (!isValidRequest.Success) continue;
				
				Task.Run(() =>
			     {
				     _jobManager.CreateJobDetail(detail, workerNodeUri);
                });
			}

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri workerNodeUri)
		{
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

			this.Log().InfoWithLineNumber($"{WhoAmI(Request)}: Received init from Node. Node Uri : ( {workerNodeUri} )");

			_nodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(workerNodeUri.ToString());
			_nodeManager.AddWorkerNode(workerNodeUri);
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IHttpActionResult Ping()
		{
			return Ok();
		}

		private static string WhoAmI(HttpRequestMessage request)
		{
			if (request == null)
			{
				return $"[MANAGER, {Environment.MachineName.ToUpper()}]";
			}

			var baseUrl =
				request.RequestUri.GetLeftPart(UriPartial.Authority);

			return $"[MANAGER, {baseUrl}, {Environment.MachineName.ToUpper()}]";
		}
	}
}