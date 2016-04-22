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

		void DeleteJobByJobId(Guid jobId, bool removeJobDetails);

		void DeleteJobQueueItemByJobId(Guid jobId);

		void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri, bool keepJobDetailsIfExists);

		void AssignJobToWorkerNode(IHttpSender httpSender);

		void CancelJobByJobId(Guid jobId, IHttpSender httpSender);

		void UpdateResultForJob(Guid jobId, string result, DateTime ended);

		void CreateJobDetailByJobId(Guid jobId, string detail, DateTime created);

		Job GetJobByJobId(Guid jobId);

		Job GetSelectJobThatDidNotEndByWorkerNodeUri(string sentToWorkerNodeUri);

		IList<Job> GetAllJobs();

		IList<Job> GetAllExecutingJobs();

		IList<Uri> GetAllDistinctSentToWorkerNodeUri();

		IList<Uri> GetDistinctSentToWorkerNodeUriForExecutingJobs();

		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
	}
}