using System;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Workers;

namespace Shared.Stardust.Node
{
    public class NodeActionExecutor
    {
        private readonly WorkerWrapperService _workerWrapperService;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerWrapperService));
        private const string JobIdIsInvalid = "Job Id is invalid."; 

        public NodeActionExecutor(WorkerWrapperService workerWrapperService)
        {
            _workerWrapperService = workerWrapperService;
        }


        public SimpleResponse PrepareToStartJob(JobQueueItemEntity jobQueueItemEntity, int portNumber)
        {
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(portNumber);
            
            var result = workerWrapper.ValidateStartJob(jobQueueItemEntity);
            if (!result.HttpResponseMessage.IsSuccessStatusCode)
            {
                return SimpleResponse.BadRequest(result.HttpResponseMessage.Content.ReadAsStringAsync().Result);
            }
            var prepareToStartJobResult = new PrepareToStartJobResult { IsAvailable = !result.IsWorking && result.HttpResponseMessage.IsSuccessStatusCode };
            return SimpleResponse.Ok(prepareToStartJobResult);
        }


        public SimpleResponse StartJob(Guid jobId, int portNumber)
        {
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(portNumber);

            if (jobId == Guid.Empty)
            {
                return SimpleResponse.BadRequest(JobIdIsInvalid);
            }

            workerWrapper.CancelTimeoutCurrentMessageTask();

            var currentMessage = workerWrapper.GetCurrentMessageToProcess();

            if (currentMessage == null)
            {
                return SimpleResponse.BadRequest("Current message has timed out.");
            }

            if (currentMessage.JobId != jobId)
            {
                return SimpleResponse.BadRequest("Current message job id does not match with job id argument.");
            }

            Task.Run(() =>
            {				
                var startJobMessage = $"{workerWrapper.WhoamI} : Starting job ( jobId, jobName ) : ( {currentMessage.JobId}, {currentMessage.Name} )";

                Logger.InfoWithLineNumber(startJobMessage);

                workerWrapper.StartJob(currentMessage);
            });

            return SimpleResponse.Ok();
        }


        public SimpleResponse TryCancelJob(Guid jobId, int portNumber)
        {
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(portNumber);

            if (jobId == Guid.Empty)
            {
                return SimpleResponse.BadRequest(JobIdIsInvalid);
            }

            Logger.InfoWithLineNumber(workerWrapper.WhoamI +
                                      " : Received Cancel request. jobId: " + jobId);
			
            var currentJob = workerWrapper.GetCurrentMessageToProcess();

            if (currentJob == null || currentJob.JobId != jobId)
            {
                Logger.WarningWithLineNumber(workerWrapper.WhoamI +
                                             ": Could not cancel job since job not found on this node. Manager sent job ( jobId ) : ( " +
                                             jobId + " )");

                return SimpleResponse.NotFound();
            }

            if (workerWrapper.IsCancellationRequested)
            {
                return SimpleResponse.Conflict();
            }

            workerWrapper.CancelJob(jobId);

            if (workerWrapper.IsCancellationRequested)
            {
                return SimpleResponse.Ok();
            }

            return SimpleResponse.NotFound();
        }

        public SimpleResponse IsWorking(int portNumber)
        {
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(portNumber);
            return SimpleResponse.Ok(workerWrapper.IsWorking);
        }

        public SimpleResponse IsIdle(int portNumber)
        {
            var workerWrapper = _workerWrapperService.GetWorkerWrapperByPort(portNumber);
            var currentJob = workerWrapper.GetCurrentMessageToProcess();
            if (currentJob == null)
            {
                return SimpleResponse.Ok();
            }
            return SimpleResponse.Conflict();
        }

    }
}
