using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
	public class FakeJobRepository : IJobRepository
	{
		private readonly List<Job> _jobs = new List<Job>();

		private readonly List<JobQueueItem> _jobQueue = new List<JobQueueItem>();

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			_jobQueue.Add(jobQueueItem);
		}

		public List<JobQueueItem> GetAllItemsInJobQueue()
		{
			return _jobQueue;
		}

		public void DeleteItemInJobQueueByJobId(Guid jobId)
		{
			var j = _jobs.FirstOrDefault(x => x.JobId.Equals(jobId));
			_jobs.Remove(j);
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
			var jobs = _jobs.FirstOrDefault(x => x.SentToWorkerNodeUri == url);

			if (jobs != null)
			{
				jobs.SentToWorkerNodeUri = "";
			}
		}

		public void TryAssignJobToWorkerNode(IHttpSender httpSender)
		{
			throw new NotImplementedException();
		}

		public void CancelJobByJobId(Guid jobId, IHttpSender httpSender)
		{
			throw new NotImplementedException();
		}

		public void SetEndResultForJob(Guid jobId, string result, DateTime ended)
		{
			throw new NotImplementedException();
		}

		public void ReportProgress(Guid jobId, string detail, DateTime created)
		{
			throw new NotImplementedException();
		}

		public Job GetJobByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllJobs()
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllExecutingJobs()
		{
			throw new NotImplementedException();
		}

		public IList<Uri> GetAllDistinctSentToWorkerNodeUri()
		{
			throw new NotImplementedException();
		}

		public IList<Uri> GetDistinctSentToWorkerNodeUriForExecutingJobs()
		{
			throw new NotImplementedException();
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public void AssignDesignatedNode(Guid id, string url)
		{
			var job = _jobs.FirstOrDefault(x => x.JobId == id);

			job.SentToWorkerNodeUri = url;
		}
	}
}