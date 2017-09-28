using System;
using System.Collections.Generic;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public interface IStardustRepository
	{
		void DeleteQueuedJobs(Guid[] jobIds);
		object GetAllFailedJobs(int from, int to);
		IList<Job> GetAllJobs(int from, int to);
		List<Job> GetAllQueuedJobs(int from, int to);
		IList<Job> GetAllRunningJobs();
		List<WorkerNode> GetAllWorkerNodes();
		Job GetJobByJobId(Guid jobId);
		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
		IList<Job> GetJobsByNodeId(Guid nodeId, int from, int to);
		Job GetQueuedJob(Guid jobId);
		List<Guid> SelectAllBus(string connString);
		List<Guid> SelectAllTenants();
		WorkerNode WorkerNode(Guid nodeId);
	}
}