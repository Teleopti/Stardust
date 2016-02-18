using System;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
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

        [HttpPost, AllowAnonymous, Route(NodeRouteConstants.Job)]
        public IHttpActionResult StartJob(JobToDo jobToDo)
        {
            if (jobToDo == null)
            {
                throw new ArgumentNullException();
            }

            if (_workerWrapper.IsTaskExecuting)
            {
                string msg = string.Format("{0} : New job request from manager rejected, node is working on another job ( jobId, jobName ) : ( {1}, {2} )",
                                           _workerWrapper.WhoamI,
                                           jobToDo.Id,
                                           jobToDo.Name);

                LogHelper.LogInfoWithLineNumber(Logger,
                                                msg);

                return CreateConflictStatusCode();
            }

            var response = _workerWrapper.StartJob(jobToDo,
                                                   Request);
            if (response.GetType() != typeof (OkResult))
            {
                return response;
            }

            string startJobMessage = string.Format("{0} : Starting job ( jobId, jobName ) : ( {1}, {2} )",
                                                   _workerWrapper.WhoamI,
                                                   jobToDo.Id,
                                                   jobToDo.Name);

            LogHelper.LogInfoWithLineNumber(Logger,
                                            startJobMessage);


        //    return CreateOkStatusCode(jobToDo);
            return Ok();
        }

        [HttpDelete, AllowAnonymous, Route(NodeRouteConstants.CancelJob)]
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                            _workerWrapper.WhoamI + ": Try cancel job ( jobId ) : ( " + jobId + " )");

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
            LogHelper.LogInfoWithLineNumber(Logger,
                                            _workerWrapper.WhoamI + ": Could not cancel job since job not found on this node. Manager sent job ( jobId ) : ( " +
                                            jobId + " )");
            return NotFound();
        }

        [HttpPost, AllowAnonymous, Route(NodeRouteConstants.IsAlive)]
        public IHttpActionResult IsAlive()
        {
            return new OkResult(Request);
        }

        private IHttpActionResult CreateOkStatusCode(JobToDo jobToDo)
        {
            ValidateJobDefintionValues(jobToDo);

            return Ok(_workerWrapper.WhoamI + ": Work started for job ( jobId, jobName )  : ( " + jobToDo.Id + ", " + jobToDo.Name + " )");
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