using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using Shared.Stardust.Manager;
using Shared.Stardust.Manager.Interfaces;
using Stardust.Manager.Constants;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ApiController, IamStardustManagerController
	{
        private readonly ManagerActionExecutor _managerActionExecutor;

        public ManagerController(ManagerActionExecutor managerActionExecutor)
        {
            _managerActionExecutor = managerActionExecutor;
        }

        private IHttpActionResult ResultTranslator(SimpleResponse simpleResponse)
        {
            
            switch (simpleResponse.Result)
            {
                case SimpleResponse.Status.Ok:
                    return Ok(simpleResponse.ResponseValue);
                case SimpleResponse.Status.BadRequest:
                    return BadRequest(simpleResponse.Message);
                case SimpleResponse.Status.Conflict:
                    return Conflict();
                case SimpleResponse.Status.InternalServerError:
                    return InternalServerError(simpleResponse.Exception);
                default:
                    throw new Exception("ResultTranslator - does not have a mapping for: " + simpleResponse.Result.ToString());
            }
        }


		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
		{
            return ResultTranslator( _managerActionExecutor.AddItemToJobQueue(jobQueueItem, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelJobByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.CancelJobByJobId(jobId, Request?.RequestUri?.GetLeftPart(UriPartial.Authority))); ;
		}

		[HttpGet, Route(ManagerRouteConstants.Jobs)]
		public IHttpActionResult GetAllJobs()
		{
            return ResultTranslator( _managerActionExecutor.GetAllJobs());
        }

		[HttpGet, Route(ManagerRouteConstants.JobByJobId)]
		public IHttpActionResult GetJobByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.GetJobByJobId(jobId));
        }

		[HttpGet, Route(ManagerRouteConstants.JobDetailByJobId)]
		public IHttpActionResult GetJobDetailsByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.GetJobDetailsByJobId(jobId));
        }

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
		{
            return ResultTranslator( _managerActionExecutor.WorkerNodeRegisterHeartbeat(workerNodeUri, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }

		[HttpPost, Route(ManagerRouteConstants.JobSucceed)]
		public IHttpActionResult JobSucceed(Guid jobId)
		{		
            return ResultTranslator( _managerActionExecutor.JobSucceed(jobId, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailed jobFailed)
		{
            return ResultTranslator( _managerActionExecutor.JobFailed(jobFailed, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }
		
		[HttpPost, Route(ManagerRouteConstants.JobCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.JobCanceled(jobId, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }


		[HttpPost, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult AddJobDetail([FromBody] IList<JobDetail> jobDetails)
		{
            return ResultTranslator( _managerActionExecutor.AddJobDetail(jobDetails));
        }

		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri workerNodeUri)
		{	
            return ResultTranslator( _managerActionExecutor.NodeInitialized(workerNodeUri, Request?.RequestUri?.GetLeftPart(UriPartial.Authority)));
        }

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IHttpActionResult Ping()
		{
			return Ok();
		}
    }
}