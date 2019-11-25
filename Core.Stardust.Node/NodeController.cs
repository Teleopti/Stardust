using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shared.Stardust.Node;
using Stardust.Node.Constants;
using Stardust.Node.Entities;

namespace Stardust.Node
{
    [ApiController]
	public class NodeController : ControllerBase, IamStardustNodeController
	{
        private readonly NodeActionExecutor _nodeActionExecutor;

        public NodeController(NodeActionExecutor nodeActionExecutor)
        {
            _nodeActionExecutor = nodeActionExecutor;
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
                default:
                    throw new Exception("ResultTranslator - does not have a mapping for: " + simpleResponse.Result.ToString());
            }
        }

        [HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
        public IActionResult PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity)
        {
            var port = GetPort();
            return ResultTranslator(_nodeActionExecutor.PrepareToStartJob(jobQueueItemEntity, port));
        }

        [HttpPut, AllowAnonymous, Route(NodeRouteConstants.UpdateJob)]
        public IActionResult StartJob(Guid jobId)
        {
            var port = GetPort();
            return ResultTranslator(_nodeActionExecutor.StartJob(jobId, port));
        }

        [HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
        public IActionResult TryCancelJob(Guid jobId)
        {
            var port = GetPort();
            return ResultTranslator(_nodeActionExecutor.StartJob(jobId, port));
        }

        [HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsAlive)]
        public IActionResult IsAlive()
        {
            return Ok();
        }

        [HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsWorking)]
        public IActionResult IsWorking()
        {
            var port = GetPort();
            return ResultTranslator(_nodeActionExecutor.IsWorking(port));
        }

        [HttpGet, AllowAnonymous, Route(NodeRouteConstants.IsIdle)]
        public IActionResult IsIdle()
        {
            var port = GetPort();
            return ResultTranslator(_nodeActionExecutor.IsIdle(port));
        }

        private int GetPort()
        {
            if (Request.Host.Port != null)
            {
                return Request.Host.Port.Value;
            }
            throw  new Exception("Port number not set!");
        }

    }
}