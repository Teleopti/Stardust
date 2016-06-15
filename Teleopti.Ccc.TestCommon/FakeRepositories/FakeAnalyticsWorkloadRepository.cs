using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsWorkloadRepository : IAnalyticsWorkloadRepository
	{
		public readonly List<AnalyticsWorkload> Workloads = new List<AnalyticsWorkload>();
		private readonly List<AnalyticsBridgeQueueWorkload> _bridgeQueueWorkloads = new List<AnalyticsBridgeQueueWorkload>();
		private int workloadIdCounter;

		public int AddOrUpdate(AnalyticsWorkload analyticsWorkload)
		{
			var existing = Workloads.FirstOrDefault(x => x.WorkloadCode == analyticsWorkload.WorkloadCode);
			var id = existing?.WorkloadId ?? ++workloadIdCounter;
			Workloads.RemoveAll(x => x.WorkloadCode == analyticsWorkload.WorkloadCode);
			analyticsWorkload.WorkloadId = id;
			Workloads.Add(analyticsWorkload);
			return id;
		}

		public void AddOrUpdateBridge(AnalyticsBridgeQueueWorkload bridgeQueueWorkload)
		{
			_bridgeQueueWorkloads.RemoveAll(
				x => x.WorkloadId == bridgeQueueWorkload.WorkloadId && x.QueueId == bridgeQueueWorkload.QueueId);
			_bridgeQueueWorkloads.Add(bridgeQueueWorkload);
		}

		public IList<AnalyticsBridgeQueueWorkload> GetBridgeQueueWorkloads(int workloadId)
		{
			return _bridgeQueueWorkloads;
		}

		public void DeleteBridge(int workloadId, int queueId)
		{
			_bridgeQueueWorkloads.RemoveAll(x => x.QueueId == queueId && x.WorkloadId == workloadId);
		}
	}
}