using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsBridgeQueueWorkload
	{
		public int QueueId { get; set; }
		public int WorkloadId { get; set; }
		public int SkillId { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}