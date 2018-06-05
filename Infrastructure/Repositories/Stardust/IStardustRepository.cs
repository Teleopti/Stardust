﻿using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Repositories.Stardust
{
	public interface IStardustRepository
	{
		void DeleteQueuedJobs(Guid[] jobIds);
		IList<Job> GetJobs(JobFilterModel filter);
		IList<Job> GetAllQueuedJobs(JobFilterModel filter);
		List<WorkerNode> GetAllWorkerNodes();
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
		int GetQueueCunt();
	}
}