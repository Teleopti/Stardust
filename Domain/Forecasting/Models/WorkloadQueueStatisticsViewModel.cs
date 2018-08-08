using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class WorkloadQueueStatisticsViewModel
	{
		public Guid WorkloadId { get; set; }
		public List<QueueStatisticsModel> QueueStatisticsDays { get; set; }
	}
}