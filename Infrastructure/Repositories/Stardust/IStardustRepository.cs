using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Stardust;

namespace Teleopti.Ccc.Infrastructure.Repositories.Stardust
{
	public interface IStardustRepository : IGetAllWorkerNodes
	{
		void DeleteQueuedJobs(Guid[] jobIds);
		IList<Job> GetJobs(JobFilterModel filter);
		IList<Job> GetAllQueuedJobs(JobFilterModel filter);
		Job GetJobByJobId(Guid jobId);
		IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
		IList<Job> GetJobsByNodeId(Guid nodeId, int from, int to);
		Job GetQueuedJob(Guid jobId);
		List<Guid> SelectAllBus(string connString);
		List<Guid> SelectAllTenants();
		WorkerNode WorkerNode(Guid nodeId);
		List<string> GetAllTypes();
		List<string> GetAllTypesInQueue();
		Job GetOldestJob();
		int GetQueueCount();
	}
}