using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IReforecastPercentCalculator
	{
		double Calculate(IWorkloadDay workloadDay, DateTime lastPeriodEndToUse);
	}

	public class ReforecastPercentCalculator : IReforecastPercentCalculator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public double Calculate(IWorkloadDay workloadDay, DateTime lastPeriodEndToUse)
		{
			var sumTasks = 0.0;
			var sumStat = 0.0;
			// how should we know if it is zero because we don't have statistic?????????
			foreach (var templateTaskPeriod in workloadDay.SortedTaskPeriodList)
			{
				if (!(templateTaskPeriod.Period.StartDateTime > lastPeriodEndToUse))
				{
					sumTasks += templateTaskPeriod.TotalTasks;
					sumStat += templateTaskPeriod.StatisticTask.StatOfferedTasks;
				}

			}

			if (sumTasks == 0.0) return 1.0;

			return sumStat / sumTasks;
		}
	}
}