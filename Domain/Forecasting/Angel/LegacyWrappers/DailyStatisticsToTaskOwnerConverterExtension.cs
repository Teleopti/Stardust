using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public static class DailyStatisticsToTaskOwnerConverterExtension
	{
		public static IEnumerable<ITaskOwner> Convert(this IEnumerable<DailyStatistic> source, IWorkload workload)
		{
			return source.Select(statistic =>
			{
				var workloadDay = new WorkloadDay();
				workloadDay.Create(statistic.Date, workload, new List<TimePeriod>());

				return new ValidatedVolumeDay(workload, statistic.Date)
				{
					ValidatedTasks = statistic.CalculatedTasks,
					ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(statistic.AverageAfterTaskTimeSeconds),
					ValidatedAverageTaskTime = TimeSpan.FromSeconds(statistic.AverageTaskTimeSeconds),
					TaskOwner = workloadDay
				};
			});
		}
	}
}