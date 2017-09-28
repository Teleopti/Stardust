using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Wfm.Administration.Core.Stardust;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.AdministrationTest
{
	public class FakeStardustRepository : IStardustRepository
	{
		private readonly List<WorkerNode> _nodes;

		public FakeStardustRepository()
		{
			_nodes = new List<WorkerNode>();
		}

		public void Has(WorkerNode node)
		{
			_nodes.Add(node);
		}

		public void Clear()
		{
			_nodes.Clear();
		}

		public void DeleteQueuedJobs(Guid[] jobIds)
		{
			throw new NotImplementedException();
		}

		public object GetAllFailedJobs(int @from, int to)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllJobs(int @from, int to)
		{
			throw new NotImplementedException();
		}

		public List<Job> GetAllQueuedJobs(int @from, int to)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetAllRunningJobs()
		{
			throw new NotImplementedException();
		}

		public List<WorkerNode> GetAllWorkerNodes()
		{
			return _nodes;
		}

		public Job GetJobByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public IList<Job> GetJobsByNodeId(Guid nodeId, int @from, int to)
		{
			throw new NotImplementedException();
		}

		public Job GetQueuedJob(Guid jobId)
		{
			throw new NotImplementedException();
		}

		public List<Guid> SelectAllBus(string connString)
		{
			throw new NotImplementedException();
		}

		public List<Guid> SelectAllTenants()
		{
			throw new NotImplementedException();
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			return _nodes.FirstOrDefault(x => x.Id == nodeId);
		}
	}
}