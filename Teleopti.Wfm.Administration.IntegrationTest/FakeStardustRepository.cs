using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Stardust;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;

namespace Teleopti.Wfm.Administration.IntegrationTest
{

	public class FakeStardustRepository : IStardustRepository
	{
		private readonly List<WorkerNode> _nodes;
		private readonly List<Job> _jobs;
		private readonly List<Job> _queuedJobs;

		public FakeStardustRepository()
		{
			_nodes = new List<WorkerNode>();
			_jobs = new List<Job>();
			_queuedJobs = new List<Job>();
		}

		public void Has(WorkerNode node)
		{
			_nodes.Add(node);
		}

		public void Has(Job job)
		{
			job.Started = DateTime.UtcNow;
			job.Ended = DateTime.UtcNow.AddSeconds(1); //Cheating
			job.Result = "Success";
			_jobs.Add(job);
		}

		public void Clear()
		{
			_nodes.Clear();
			_jobs.Clear();
		}

		public void DeleteQueuedJobs(Guid[] jobIds)
		{
			
		}

		public IList<Job> GetJobs(JobFilterModel filter)
		{
			return new List<Job>();
		}

		public IList<Job> GetAllQueuedJobs(JobFilterModel filter)
		{
			return _queuedJobs;
		}

		public IEnumerable<WorkerNode> GetAllWorkerNodes()
		{
			return _nodes;
		}

		public Job GetJobByJobId(Guid jobId)
		{
			return _jobs.FirstOrDefault(x => x.JobId == jobId);
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			return  new List<JobDetail>();
		}

		public IList<Job> GetJobsByNodeId(Guid nodeId, int @from, int to)
		{
			return new List<Job>();
		}

		public Job GetQueuedJob(Guid jobId)
		{
			return null;
		}

		public List<Guid> SelectAllBus(string connString)
		{
			return new List<Guid>(){Guid.NewGuid()};
		}

		public List<Guid> SelectAllTenants()
		{
			throw new NotImplementedException();
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			return _nodes.FirstOrDefault(x => x.Id == nodeId);
		}

		public List<string> GetAllTypes()
		{
			throw new NotImplementedException();
		}

		public List<string> GetAllTypesInQueue()
		{
			throw new NotImplementedException();
		}

		public Job GetOldestJob()
		{
			throw new NotImplementedException();
		}

		public int GetQueueCount()
		{
			throw new NotImplementedException();
		}
	}
}