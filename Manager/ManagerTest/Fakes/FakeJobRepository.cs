using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
	public class FakeJobRepository : IJobRepository
	{
        public readonly List<DateTime> HasCalledAssignJobToNode = new List<DateTime>();
        public readonly List<JobDetail> JobDetailList = new List<JobDetail>();
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

		public void AssignJobToWorkerNode()
		{
			HasCalledAssignJobToNode.Add(DateTime.Now);
		}

		public void CancelJobByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public void UpdateResultForJob(Guid jobId, string result, string workerNodeUri, DateTime ended)
        {
            //var currentJob = _jobs.Single(j => j.JobId == jobId)
            //currentJob.Result = result;
            //currentJob.Ended = ended;
        }

		public void CreateJobDetailByJobId(Guid jobId, string detail, string workerNodeUri, DateTime created)
        {
            JobDetailList.Add(new JobDetail {Created = created, Detail = detail, JobId = jobId});
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
            return JobDetailList;
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