using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
	public class FakeJobRepository : IJobRepository
	{
		private readonly List<JobDefinition> _jobs = new List<JobDefinition>();

		public void AddJobDefinition(JobDefinition jobDefinition)
		{
			_jobs.Add(jobDefinition);
		}

		public List<JobDefinition> GetAllJobDefinitions()
		{
			return _jobs;
		}

		public void DeleteJobByJobId(Guid jobId)
		{
			var j = _jobs.FirstOrDefault(x => x.Id.Equals(jobId));
			_jobs.Remove(j);
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
			var jobs = _jobs.FirstOrDefault(x => x.AssignedNode == url);
			if (jobs != null)
			{
				jobs.AssignedNode = "";
			}
		}

		public void CheckAndAssignNextJob(IHttpSender httpSender)
		{
			throw new NotImplementedException();
		}

		public void CancelJobByJobId(Guid jobId, IHttpSender httpSender)
		{
			throw new NotImplementedException();
		}

		public void SetEndResultOnJob(Guid jobId, string result)
		{
			throw new NotImplementedException();
		}

		public void ReportProgress(Guid jobId, string detail, DateTime created)
		{
			throw new NotImplementedException();
		}

		public JobHistory GetJobHistoryByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public IList<JobHistory> GetAllJobHistories()
		{
			throw new NotImplementedException();
		}

		public IList<JobHistoryDetail> GetJobHistoryDetailsByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public void AssignDesignatedNode(Guid id, string url)
		{
			var job = _jobs.FirstOrDefault(x => x.Id == id);
			job.AssignedNode = url;
		}
	}
}