using System;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.API
{
    public class NodeController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeController));

        private readonly IWorkerWrapper _workerWrapper;

        public NodeController(IWorkerWrapper workerWrapper)
        {
            _workerWrapper = workerWrapper;
        }

        [HttpPost, Route(NodeRouteConstants.Job)]
        public IHttpActionResult StartJob(JobToDo jobToDo)
        {
            if (jobToDo == null)
            {
                throw new ArgumentNullException();
            }
            if (_workerWrapper.IsTaskExecuting)
            {
                Logger.Info(_workerWrapper.WhoamI +
                            ": New job request from manager rejected, node is working on an earlier request. JobId: " +
                            jobToDo.Id);
                return CreateConflictStatusCode();
            }

            var response = _workerWrapper.StartJob(jobToDo,
                                                   Request);
            if (response.GetType() != typeof (OkResult))
            {
                return response;
            }
            Logger.Info(_workerWrapper.WhoamI + ": Starting Job. JobId " + jobToDo.Id);

            return CreateOkStatusCode(jobToDo);
        }

        [HttpDelete, Route(NodeRouteConstants.CancelJob)]
        public IHttpActionResult TryCancelJob(Guid jobId)
        {
            if (jobId == null)
            {
                throw new ArgumentNullException();
            }
            if (jobId == Guid.Empty)
            {
                throw new ArgumentNullException();
            }
            Logger.Info(_workerWrapper.WhoamI + ": Try cancel job. JobId " + jobId);

            var currentJob = _workerWrapper.GetCurrentMessageToProcess();

            if (currentJob == null || currentJob.Id != jobId)
            {
                return NotFound();
            }
            if (_workerWrapper.IsCancellationRequested)
            {
                return Conflict();
            }

            _workerWrapper.CancelJob(jobId);

            if (_workerWrapper.IsCancellationRequested)
            {
                return Ok();
            }
            Logger.Info(_workerWrapper.WhoamI + ": Could not cancel job since job not found on this node. JobId " +
                        jobId);
            return NotFound();
        }

        [HttpPost, Route(NodeRouteConstants.IsAlive)]
        public IHttpActionResult IsAlive()
        {
            return new OkResult(Request);
        }

        private IHttpActionResult CreateOkStatusCode(JobToDo jobToDo)
        {
            ValidateJobDefintionValues(jobToDo);

            return Ok(_workerWrapper.WhoamI + ": Work started for jobId " + jobToDo.Name);
        }

        private IHttpActionResult CreateConflictStatusCode()
        {
            return Conflict();
        }

        private static void ValidateJobDefintionValues(JobToDo jobToDo)
        {
            jobToDo.ThrowExceptionWhenNull();
            jobToDo.Name.ThrowArgumentExceptionIfNullOrEmpty();
        }
    }
}