using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsWorkloadRepository
	{
		int AddOrUpdate(AnalyticsWorkload analyticsWorkload);
		void AddOrUpdateBridge(AnalyticsBridgeQueueWorkload bridgeQueueWorkload);
		IList<AnalyticsBridgeQueueWorkload> GetBridgeQueueWorkloads(int workloadId);
		void DeleteBridge(int workloadId, int queueId);
		AnalyticsWorkload GetWorkload(Guid workloadCode);
	}
}