using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Newtonsoft.Json;
using Shared.Stardust.Node;
using Shared.Stardust.Node.Workers;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node
{
    
	public class NodeController : ApiController, IamStardustNodeController
	{
		private readonly WorkerWrapperService _workerWrapperService;
        private readonly NodeActionExecutor _nodeActionExecutor;
        private const string JobIdIsInvalid = "Job Id is invalid."; 
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

		public NodeController(WorkerWrapperService workerWrapperService, NodeActionExecutor nodeActionExecutor)
        {
            _workerWrapperService = workerWrapperService;
            _nodeActionExecutor = nodeActionExecutor;
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
                default:
                    throw new Exception("ResultTranslator - does not have a mapping for: " + simpleResponse.Result.ToString());
            }
        }


		[HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
		public IHttpActionResult PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
        {
            var port = ActionContext.Request.RequestUri.Port;
			return Ok(_nodeActionExecutor.PrepareToStartJob(jobQueueItemEntity, port));
		}

		[HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
		public IHttpActionResult StartJob(Guid jobId)
		{
            var port = ActionContext.Request.RequestUri.Port;
            return  ResultTranslator(_nodeActionExecutor.StartJob(jobId, port));
        }

		[HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
		public IHttpActionResult TryCancelJob(Guid jobId)
		{
            var port = ActionContext.Request.RequestUri.Port;
            return ResultTranslator(_nodeActionExecutor.StartJob(jobId, port));
			
		}


		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsAlive)]
		public IHttpActionResult IsAlive()
		{
			return Ok();
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsWorking)]
		public IHttpActionResult IsWorking()
		{
            var port = ActionContext.Request.RequestUri.Port;
            return ResultTranslator(_nodeActionExecutor.IsWorking(port));
		}

		[HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
		public IHttpActionResult IsIdle()
		{
			var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(ActionContext.Request.RequestUri.Port);

			var currentJob = workerWrapper.GetCurrentMessageToProcess();

			if (currentJob == null)
			{
				return Ok();
			}

			return Conflict();
		}
	}
}