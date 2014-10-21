using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public static class DailyStatisticsToTaskOwnerConverterExtension
	{
		public static IEnumerable<ITaskOwner> Convert(this IEnumerable<DailyStatistic> source,IWorkload workload)
		{
			return source.Select(s =>
			{
				var workloadDay = new WorkloadDay();
				workloadDay.Create(DateOnly.Today,workload,new List<TimePeriod>());

				return new ValidatedVolumeDay(workload,s.Date)
				{
					ValidatedTasks = s.CalculatedTasks,
					TaskOwner = workloadDay
				};
			});
		}
	}
}