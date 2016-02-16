using System;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Stardust.Manager.Constants;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
    public class ManagerController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ManagerController));

        private readonly INodeManager _nodeManager;
        private readonly JobManager _jobManager;

        public string WhoAmI { get; private set; }

        public ManagerController(INodeManager nodeManager,
                                 JobManager jobManager)
        {
            WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";

            _nodeManager = nodeManager;
            _jobManager = jobManager;
        }

      //  [HttpPost, Route(ManagerRouteConstants.StartJob)]
      [HttpPost, ActionName("job")]
        public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
        {
            if (job == null)
            {
                return new BadRequestResult(Request);
            }

            if (string.IsNullOrEmpty(job.Name) || 
                string.IsNullOrEmpty(job.Serialized) || 
                string.IsNullOrEmpty(job.Type) || 
                string.IsNullOrEmpty(job.UserName))
            {
                return new BadRequestResult(Request);
            }

            var jobReceived = new JobDefinition()
            {
                Name = job.Name,
                Serialized = job.Serialized,
                Type = job.Type,
                UserName = job.UserName,
                //Should we do it here or?
                Id = Guid.NewGuid()
            };
            _jobManager.Add(jobReceived);

            string msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
                                       WhoAmI,
                                       jobReceived.Id,
                                       jobReceived.Name);

            LogHelper.LogInfoWithLineNumber(Logger,
                                            msg);

            return Ok(jobReceived.Id);
        }

        //   [HttpPost, Route(ManagerRouteConstants.Heartbeat)]
        [HttpPost, ActionName("heartbeat")]
        public void Heartbeat([FromBody] Uri nodeUri)
        {
            _jobManager.CheckAndAssignNextJob();

            LogHelper.LogInfoWithLineNumber(Logger,
                                           WhoAmI + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");
        }


        //   [HttpDelete, Route(ManagerRouteConstants.CancelJob)]
        [HttpDelete, ActionName("job")]
        public void CancelThisJob(Guid jobId)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            WhoAmI + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

            _jobManager.CancelThisJob(jobId);
        }

    //    [HttpPost, Route(ManagerRouteConstants.JobDone)]
        [HttpPost, ActionName("done")]
        public IHttpActionResult JobDone(Guid jobId)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            WhoAmI + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

            _jobManager.SetEndResultOnJobAndRemoveIt(jobId,
                                                     "Success");

            //should we do this here also or only on heartbeats
            _jobManager.CheckAndAssignNextJob();

            return Ok();
        }

        //        [HttpPost, Route(ManagerRouteConstants.JobFailed)]
        [HttpPost, ActionName("fail")]
        public IHttpActionResult JobFailed(Guid jobId)
        {
            LogHelper.LogErrorWithLineNumber(Logger,
                                             WhoAmI + ": Received job failed from a Node ( jobId ) : ( " + jobId + " )");

            _jobManager.SetEndResultOnJobAndRemoveIt(jobId,
                                                     "Failed");
            return Ok();
        }

        //    [HttpPost, Route(ManagerRouteConstants.JobHasBeenCanceled)]
        [HttpPost, ActionName("cancel")]
        public IHttpActionResult JobCanceled(Guid jobId)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            WhoAmI + ": Received cancel from a Node ( jobId ) : ( " + jobId + " )");

            _jobManager.SetEndResultOnJobAndRemoveIt(jobId,
                                                     "Canceled");

            return Ok();
        }

        //  [HttpPost, Route(ManagerRouteConstants.JobProgress)]
        [HttpPost, ActionName("progress")]
        public IHttpActionResult JobProgress([FromBody] JobProgressModel model)
      {
            _jobManager.ReportProgress(model);

            return Ok();
        }

        // to handle that scenario where the node comes up after a crash
        //this end point should be called when the node comes up
        //  [HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
        [HttpPost, ActionName("nodeinit")]
        public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
        {
           
            _nodeManager.FreeJobIfAssingedToNode(nodeUri);
            _nodeManager.AddIfNeeded(nodeUri);

            LogHelper.LogInfoWithLineNumber(Logger,
                                           WhoAmI + ": Received init from Node. Node Uri : ( " + nodeUri + " )");

            return Ok();
        }

        //     [HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
        [HttpGet]
        public IHttpActionResult JobHistory(Guid jobId)
        {
            JobHistory jobHistory = _jobManager.GetJobHistory(jobId);

            return Ok(jobHistory);
        }
        
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok();
        }
    }
}