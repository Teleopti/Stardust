using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IJobRepository
	{
		bool DoesJobQueueItemExists(Guid jobId);

		JobQueueItem GetJobQueueItemByJobId(Guid jobId);

		void AddItemToJobQueue(JobQueueItem jobQueueItem);

		List<JobQueueItem> GetAllItemsInJobQueue();

		void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri);

		void AssignJobToWorkerNode();

		void CancelJobByJobId(Guid jobId );

		void UpdateResultForJob(Guid jobId, string result, string workerNodeUri, DateTime ended);

		void CreateJobDetailByJobId(Guid jobId, string detail, string workerNodeUri, DateTime created);

		Job GetJobByJobId(Guid jobId);

		IList<Job> GetAllJobs();

		IList<Job> GetAllExecutingJobs();

		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
		bool PingWorkerNode(Uri workerNodeUri);
	}
}