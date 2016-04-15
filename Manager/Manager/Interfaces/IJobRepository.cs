using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IJobRepository
	{
		void AddItemToJobQueue(JobQueueItem jobQueueItem);

		List<JobQueueItem> GetAllItemsInJobQueue();

		void DeleteItemInJobQueueByJobId(Guid jobId);

		void FreeJobIfNodeIsAssigned(string url);

		void TryAssignJobToWorkerNode(IHttpSender httpSender);

		void CancelJobByJobId(Guid jobId, IHttpSender httpSender);

		void SetEndResultForJob(Guid jobId, string result, DateTime ended);

		void ReportProgress(Guid jobId, string detail, DateTime created);

		Job GetJobByJobId(Guid jobId);

		IList<Job> GetAllJobs();

		IList<Job> GetAllExecutingJobs();

		IList<Uri> GetAllDistinctSentToWorkerNodeUri();

		IList<Uri> GetDistinctSentToWorkerNodeUriForExecutingJobs();

		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
	}
}