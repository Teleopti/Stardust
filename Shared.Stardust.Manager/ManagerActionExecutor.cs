using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Stardust.Manager;
using Stardust.Manager.Extensions;
using Stardust.Manager.Models;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerActionExecutor
	{
		private readonly IJobManager _jobManager;
		private readonly NodeManager _nodeManager;
        private readonly Validator _validator;

		public ManagerActionExecutor(NodeManager nodeManager,
                                 IJobManager jobManager,
		                         Validator validator)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
			_validator = validator;
		}

		
		public SimpleResponse AddItemToJobQueue(JobQueueItem jobQueueItem, string hostName)
		{
			jobQueueItem.JobId = Guid.NewGuid();

			var isValidRequest = _validator.ValidateObject(jobQueueItem);
			if (!isValidRequest.Success)  return SimpleResponse.BadRequest(isValidRequest.Message);
			
			_jobManager.AddItemToJobQueue(jobQueueItem);

			var msg = $"{WhoAmI(hostName)} : New job received from client ( jobId, jobName ) : ( {jobQueueItem.JobId}, {jobQueueItem.Name} )";
			this.Log().InfoWithLineNumber(msg);

			
			Task.Run(() =>
			{
				_jobManager.AssignJobToWorkerNodes();
			});

			return SimpleResponse.Ok(jobQueueItem.JobId);
		}

		
		public SimpleResponse CancelJobByJobId(Guid jobId, string hostName)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);
			
			this.Log().InfoWithLineNumber($"{WhoAmI(hostName)}: Received job cancel from client ( jobId ) : ( {jobId} )");
			
			_jobManager.CancelJobByJobId(jobId);

			return SimpleResponse.Ok();
		}

		
		public SimpleResponse GetAllJobs()
		{
			var allJobs = _jobManager.GetAllJobs();
            return SimpleResponse.Ok(allJobs);
		}

		
		public SimpleResponse GetJobByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);
			
			var job = _jobManager.GetJobByJobId(jobId);

			return SimpleResponse.Ok(job);
		}

		
		public SimpleResponse GetJobDetailsByJobId(Guid jobId)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);
			
			var jobDetailsByJobId = _jobManager.GetJobDetailsByJobId(jobId);

			return SimpleResponse.Ok(jobDetailsByJobId);
		}

	
		public SimpleResponse WorkerNodeRegisterHeartbeat(Uri workerNodeUri, string hostName)
		{
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);

		    Task.Run(() =>
			{
				this.Log().InfoWithLineNumber(
                    $"{WhoAmI(hostName)}: Received heartbeat from Node. Node Uri : ( {workerNodeUri} )");

				_nodeManager.WorkerNodeRegisterHeartbeat(workerNodeUri.ToString());
				
			});
         
            return SimpleResponse.Ok();
		}

		
		public SimpleResponse JobSucceed(Guid jobId, string hostName)
		{			
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);

            var task = Task.Run(() =>
			{
                this.Log().InfoWithLineNumber(
                    $"{WhoAmI(hostName)}: Received job done from a Node ( jobId, Node ) : ( {jobId}, {hostName} )");

				_jobManager.UpdateResultForJob(jobId,
				                              "Success",
											  DateTime.UtcNow);

				_jobManager.AssignJobToWorkerNodes();
			});

            try
            {
                task.Wait();
            }
            catch (Exception exception)
            {
                return SimpleResponse.InternalServerError(exception);
            }

            return SimpleResponse.Ok();
		}

		
		public SimpleResponse JobFailed(JobFailed jobFailed, string hostName)
		{
			var isValidRequest = _validator.ValidateObject(jobFailed);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);

			var task = Task.Run(() =>
            {
                this.Log().ErrorWithLineNumber(
                    $"{WhoAmI(hostName)}: Received job failed from a Node ( jobId, Node ) : ( {jobFailed.JobId}, {hostName??"Unknown Uri"} )");

                var progress = new JobDetail
                {
                    JobId = jobFailed.JobId,
                    Created = DateTime.UtcNow,
                    Detail = jobFailed.AggregateException?.ToString()??"No Exception specified for job"
                };

                _jobManager.CreateJobDetail(progress);

                _jobManager.UpdateResultForJob(jobFailed.JobId,
                    "Failed",
                    DateTime.UtcNow);

                _jobManager.AssignJobToWorkerNodes();
            });

            try
            {
                var taskIsSuccessful = task.Wait(TimeSpan.FromMinutes(1));
                if (!taskIsSuccessful)
                {
                    var aggregationException = new AggregateException(
                        new TimeoutException($"Timeout while executing {nameof(JobFailed)}"));
                    return SimpleResponse.InternalServerError(aggregationException);
                }
            }
            catch (Exception exception)
            {
                return SimpleResponse.InternalServerError(exception);
            }

            return SimpleResponse.Ok();
        }
		
	
		public SimpleResponse JobCanceled(Guid jobId, string hostName)
		{
			var isValidRequest = _validator.ValidateJobId(jobId);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);
			
			Task.Run(() =>
			{
				

				this.Log().InfoWithLineNumber(
                    $"{WhoAmI(hostName)}: Received cancel from a Node ( jobId, Node ) : ( {jobId}, {hostName})");

				_jobManager.UpdateResultForJob(jobId,
				                              "Canceled",
											  DateTime.UtcNow);
				
				_jobManager.AssignJobToWorkerNodes();
			});

            return SimpleResponse.Ok();
        }


	
		public SimpleResponse AddJobDetail(IList<JobDetail> jobDetails)
		{
			foreach (var detail in jobDetails)
			{
				var isValidRequest = _validator.ValidateObject(detail);
				if (!isValidRequest.Success) continue;
				
				Task.Run(() =>
			     {
				      _jobManager.CreateJobDetail(detail);
                });
			}

			return SimpleResponse.Ok();
		}

		public SimpleResponse NodeInitialized(Uri workerNodeUri, string hostName)
		{
			var isValidRequest = _validator.ValidateUri(workerNodeUri);
			if (!isValidRequest.Success) return SimpleResponse.BadRequest(isValidRequest.Message);

			this.Log().InfoWithLineNumber($"{WhoAmI(hostName)}: Received init from Node. Node Uri : ( {hostName} )");

			_nodeManager.RequeueJobsThatDidNotFinishedByWorkerNodeUri(workerNodeUri.ToString());
			_nodeManager.AddWorkerNode(workerNodeUri);
			return SimpleResponse.Ok();
		}


        private static string WhoAmI(string host)
        {
            if (host == null)
            {
                return $"[MANAGER, {Environment.MachineName.ToUpper()}]";
            }

            return $"[MANAGER, {host}, {Environment.MachineName.ToUpper()}]";
        }


	}
}