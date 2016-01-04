using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingTargetMerger : IForecastingTargetMerger
	{
		public void Merge(IEnumerable<IForecastingTarget> forecastingTargets, IEnumerable<ITaskOwner> workloadDays)
		{
			var workloadDayForDate = workloadDays.ToLookup(w => w.CurrentDate);
			var taskOwnerHelper = new TaskOwnerHelper(workloadDays);
			taskOwnerHelper.BeginUpdate();
			foreach (var target in forecastingTargets)
			{
				var daysForDate = workloadDayForDate[target.CurrentDate];
				foreach (var workloadDay in daysForDate)
				{
					if (workloadDay.OpenForWork.IsOpen)
					{
						workloadDay.Tasks = target.Tasks;
						workloadDay.AverageTaskTime = target.AverageTaskTime;
						workloadDay.AverageAfterTaskTime = target.AverageAfterTaskTime;
					}
				}
			}
			taskOwnerHelper.EndUpdate();
		}
	}
}