using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IJobRepository
	{
		bool DoesJobQueueItemExists(Guid jobId);

		bool DoesJobItemExists(Guid jobId);

		bool DoesJobDetailItemExists(Guid jobId);

		JobQueueItem GetJobQueueItemByJobId(Guid jobId);

		void AddItemToJobQueue(JobQueueItem jobQueueItem);

		List<JobQueueItem> GetAllItemsInJobQueue();

		void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri);

		void AssignJobToWorkerNode(IHttpSender httpSender);

		void CancelJobByJobId(Guid jobId, IHttpSender httpSender);

		void UpdateResultForJob(Guid jobId, string result, DateTime ended);

		void CreateJobDetailByJobId(Guid jobId, string detail, DateTime created);

		Job GetJobByJobId(Guid jobId);

		IList<Job> GetAllJobs();

		IList<Job> GetAllExecutingJobs();

		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
	}
}