using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IJobRepository
	{
		void Add(JobDefinition job);
		List<JobDefinition> LoadAll();
		void DeleteJob(Guid jobId);
		void FreeJobIfNodeIsAssigned(string url);
		void CheckAndAssignNextJob(List<WorkerNode> availableNodes, IHttpSender httpSender);
		void CancelThisJob(Guid jobId, IHttpSender httpSender);

		void SetEndResultOnJob(Guid jobId, string result);

		void ReportProgress(Guid jobId, string detail, DateTime created);
		JobHistory History(Guid jobId);
		IList<JobHistory> HistoryList();
		IList<JobHistoryDetail> JobHistoryDetails(Guid jobId);
	}
}