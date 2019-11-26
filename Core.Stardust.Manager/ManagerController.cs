using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Stardust.Manager;
using Shared.Stardust.Manager.Interfaces;
using Stardust.Manager.Constants;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ControllerBase, IamStardustManagerController
	{
        private readonly ManagerActionExecutor _managerActionExecutor;

        public ManagerController(ManagerActionExecutor managerActionExecutor)
        {
            _managerActionExecutor = managerActionExecutor;
        }
        
        private IActionResult ResultTranslator(SimpleResponse simpleResponse)
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
                    return  StatusCode(StatusCodes.Status500InternalServerError, simpleResponse.Exception );
                default:
                    throw new Exception("ResultTranslator - does not have a mapping for: " + simpleResponse.Result.ToString());
            }
        }
        
        [HttpPost, Route(ManagerRouteConstants.Job)]
		public IActionResult AddItemToJobQueue([FromBody] JobQueueItem jobQueueItem)
        {
            return ResultTranslator( _managerActionExecutor.AddItemToJobQueue(jobQueueItem, Request.Host.Value));
        }

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IActionResult CancelJobByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.CancelJobByJobId(jobId, Request.Host.Value));
        }

		[HttpGet, Route(ManagerRouteConstants.Jobs)]
		public IActionResult GetAllJobs()
		{
            return ResultTranslator( _managerActionExecutor.GetAllJobs());
		}

		[HttpGet, Route(ManagerRouteConstants.JobByJobId)]
		public IActionResult GetJobByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.GetJobByJobId(jobId));
        }

		[HttpGet, Route(ManagerRouteConstants.JobDetailByJobId)]
		public IActionResult GetJobDetailsByJobId(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.GetJobDetailsByJobId(jobId));
        }

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IActionResult WorkerNodeRegisterHeartbeat([FromBody] Uri workerNodeUri)
		{
            return ResultTranslator( _managerActionExecutor.WorkerNodeRegisterHeartbeat(workerNodeUri, Request.Host.Host));
        }

		[HttpPost, Route(ManagerRouteConstants.JobSucceed)]
		public IActionResult JobSucceed(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.JobSucceed(jobId, Request.Host.Host));
        }

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IActionResult JobFailed([FromBody] JobFailed jobFailed)
		{
            return ResultTranslator( _managerActionExecutor.JobFailed(jobFailed, Request.Host.Host));
        }
		
		[HttpPost, Route(ManagerRouteConstants.JobCanceled)]
		public IActionResult JobCanceled(Guid jobId)
		{
            return ResultTranslator( _managerActionExecutor.JobCanceled(jobId, Request.Host.Host));
        }


		[HttpPost, Route(ManagerRouteConstants.JobDetail)]
		public IActionResult AddJobDetail([FromBody] IList<JobDetail> jobDetails)
		{
            return ResultTranslator( _managerActionExecutor.AddJobDetail(jobDetails));
        }

		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IActionResult NodeInitialized([FromBody] Uri workerNodeUri)
		{
            return ResultTranslator( _managerActionExecutor.NodeInitialized(workerNodeUri, Request.Host.Host));
        }

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IActionResult Ping()
		{
			return Ok();
		}
    }
}