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

		public bool DoesJobQueueItemExists(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public bool DoesJobItemExists(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public bool DoesJobDetailItemExists(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public JobQueueItem GetJobQueueItemByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			_jobQueue.Add(jobQueueItem);
		}

		public List<JobQueueItem> GetAllItemsInJobQueue()
		{
			return _jobQueue;
		}

		public void DeleteJobQueueItemByJobId(Guid jobId)
		{
			var j = _jobs.FirstOrDefault(x => x.JobId.Equals(jobId));
			_jobs.Remove(j);
		}

		public void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri)
		{
			var jobs = _jobs.FirstOrDefault(x => x.SentToWorkerNodeUri == workerNodeUri);

			if (jobs != null)
			{
				jobs.SentToWorkerNodeUri = "";
			}
		}

		public void AssignJobToWorkerNode( )
		{
			throw new NotImplementedException();
		}

		public void CancelJobByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public void UpdateResultForJob(Guid jobId, string result, DateTime ended)
		{
			throw new NotImplementedException();
		}

		public void CreateJobDetailByJobId(Guid jobId, string detail, DateTime created)
		{
			throw new NotImplementedException();
		}

		public Job GetJobByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public Job GetSelectJobThatDidNotEndByWorkerNodeUri(string sentToWorkerNodeUri)
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

		public bool PingWorkerNode(Uri workerNodeUri)
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