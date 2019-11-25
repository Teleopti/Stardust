using System;
using System.Web.Http;
using Shared.Stardust.Node;
using Stardust.Node.Constants;
using Stardust.Node.Entities;

namespace Stardust.Node
{
    
	public class NodeController : ApiController, IamStardustNodeController
	{
        private readonly NodeActionExecutor _nodeActionExecutor;

		public NodeController( NodeActionExecutor nodeActionExecutor)
        {
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
			return ResultTranslator(_nodeActionExecutor.PrepareToStartJob(jobQueueItemEntity, port));
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
            var port = ActionContext.Request.RequestUri.Port;
            return ResultTranslator(_nodeActionExecutor.IsIdle(port));
        }
	}
}