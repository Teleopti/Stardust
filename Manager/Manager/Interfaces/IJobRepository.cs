using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IJobRepository
	{
		void AddJobDefinition(JobDefinition jobDefinition);
		List<JobDefinition> GetAllJobDefinitions();
		void DeleteJobByJobId(Guid jobId);
		void FreeJobIfNodeIsAssigned(string url);
		void CheckAndAssignNextJob(IHttpSender httpSender);
		void CancelJobByJobId(Guid jobId, IHttpSender httpSender);

		void SetEndResultOnJob(Guid jobId, string result);

		void ReportProgress(Guid jobId, string detail, DateTime created);
		JobHistory GetJobHistoryByJobId(Guid jobId);
		IList<JobHistory> GetAllJobHistories();
		IList<JobHistoryDetail> GetJobHistoryDetailsByJobId(Guid jobId);
	}
}