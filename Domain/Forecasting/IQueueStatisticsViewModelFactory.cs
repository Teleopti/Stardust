using System;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IQueueStatisticsViewModelFactory
	{
		WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input);
	}
}