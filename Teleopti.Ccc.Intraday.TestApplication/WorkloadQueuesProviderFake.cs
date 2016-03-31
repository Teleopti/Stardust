using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class WorkloadQueuesProviderFake : IWorkloadQueuesProvider
	{
		public IList<WorkloadInfo> Provide()
		{
			return new List<WorkloadInfo>
			{
				new WorkloadInfo
				{
					WorkloadId = 8,
					Queues = new List<QueueInfo>()
					{
						new QueueInfo() {QueueId = 1, HasDataToday = true},
						new QueueInfo() {QueueId = 2, HasDataToday = false},
						new QueueInfo() {QueueId = 3, HasDataToday = false},
					}
				},
				new WorkloadInfo
				{
					WorkloadId = 9,
					Queues = new List<QueueInfo>()
					{
						new QueueInfo() {QueueId = 1, HasDataToday = true},
					}
				}
			};
		}
	}
}