using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if NET472
using System.Net.Http;
using System.Web.Http;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif
using Stardust.Manager.Constants;
using Stardust.Manager.Extensions;
using Stardust.Manager.Models;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
#if NET472
	public class ManagerController : ApiController
#else
    public class ManagerController : Controller
#endif
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
#if NET472
		public IHttpActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
#else
        public ActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
#endif
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
#if NET472
		public IHttpActionResult CancelJobByJobId(Guid jobId)
#else
        public ActionResult CancelJobByJobId(Guid jobId)
#endif
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");
			
			_jobManager.CancelJobByJobId(jobId);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Jobs)]
#if NET472
		public IHttpActionResult GetAllJobs()
#else
        public ActionResult GetAllJobs()
#endif
		{
			var allJobs = _jobManager.GetAllJobs();

			return Ok(allJobs);
		}

		[HttpGet, Route(ManagerRouteConstants.JobByJobId)]
#if NET472
		public IHttpActionResult GetJobByJobId(Guid jobId)
#else
        public ActionResult GetJobByJobId(Guid jobId)
#endif
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			var job = _jobManager.GetJobByJobId(jobId);

			return Ok(job);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetailByJobId)]
#if NET472
		public IHttpActionResult GetJobDetailsByJobId(Guid jobId)
#else
        public ActionResult GetJobDetailsByJobId(Guid jobId)
#endif
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			var jobDetailsByJobId = _jobManager.GetJobDetailsByJobId(jobId);

			return Ok(jobDetailsByJobId);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
#if NET472
		public IHttpActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
#else
        public ActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
#endif
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
#if NET472
		public IHttpActionResult JobSucceed(Guid jobId)
#else
        public ActionResult JobSucceed(Guid jobId)
#endif
		{			
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

            var task = Task.Run(() =>
			{
#if NET472
			    var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
#else
                var workerNodeUri = Request.Host.ToString();
#endif

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
#if NET472
            catch (Exception exception)
            {
				return InternalServerError(exception);
#else
            catch (Exception)
            {
                throw;
#endif
			}

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
#if NET472
		public IHttpActionResult JobFailed([FromBody] JobFailed jobFailed)
#else
        public ActionResult JobFailed([FromBody] JobFailed jobFailed)
#endif
		{
			var isValidRequest = _validator.ValidateObject(jobFailed);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

			var task = Task.Run(() =>
            {
#if NET472
			    var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
#else
                var workerNodeUri = Request.Host.ToString();
#endif

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
#if NET472
					return InternalServerError(aggregationException);
#else
                    throw aggregationException;
#endif
                }
            }
#if NET472
            catch (Exception exception)
            {
				return InternalServerError(exception);
#else
            catch (Exception)
            {
                throw;
#endif
			}

			return Ok();
        }
		
		[HttpPost, Route(ManagerRouteConstants.JobCanceled)]
#if NET472
		public IHttpActionResult JobCanceled(Guid jobId)
#else
        public ActionResult JobCanceled(Guid jobId)
#endif
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);
			
			Task.Run(() =>
			{
#if NET472
			    var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
#else
                var workerNodeUri = Request.Host.ToString();
#endif

				this.Log().InfoWithLineNumber(
                    $"{WhoAmI(Request)}: Received cancel from a Node ( jobId, Node ) : ( {jobId}, {workerNodeUri})");

				_jobManager.UpdateResultForJob(jobId,
				                              "Canceled",
				                              workerNodeUri,
											  DateTime.UtcNow);
			});

            return Ok();
        }


		[HttpPost, Route(ManagerRouteConstants.JobDetail)]
#if NET472
		public IHttpActionResult AddJobDetail([FromBody] IList<JobDetail> jobDetails)
#else
        public ActionResult AddJobDetail([FromBody] IList<JobDetail> jobDetails)
#endif
		{
#if NET472
			var workerNodeUri = Request.RequestUri.GetLeftPart(UriPartial.Authority);
#else
            var workerNodeUri = Request.Host.ToString();
#endif
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
#if NET472
		public IHttpActionResult NodeInitialized([FromBody] Uri workerNodeUri)
#else
        public ActionResult NodeInitialized([FromBody] Uri workerNodeUri)
#endif
		{
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success) return BadRequest(isValidRequest.Message);

			this.Log().InfoWithLineNumber($"{WhoAmI(Request)}: Received init from Node. Node Uri : ( {workerNodeUri} )");

			_nodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(workerNodeUri.ToString());
			_nodeManager.AddWorkerNode(workerNodeUri);
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Ping)]
#if NET472
		public IHttpActionResult Ping()
#else
        public ActionResult Ping()
#endif
		{
			return Ok();
		}

#if NET472
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
#else
        private static string WhoAmI(HttpRequest request)
        {
            if (request == null)
            {
                return $"[MANAGER, {Environment.MachineName.ToUpper()}]";
            }

            var baseUrl = request.Host;

            return $"[MANAGER, {baseUrl}, {Environment.MachineName.ToUpper()}]";
        }
#endif
	}
}