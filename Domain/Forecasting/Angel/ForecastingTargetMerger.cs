using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingTargetMerger : IForecastingTargetMerger
	{
		public void Merge(IList<IForecastingTarget> forecastingTargets, IEnumerable<ITaskOwner> workloadDays)
		{
			var taskOwnerHelper = new TaskOwnerHelper(new List<ITaskOwner>(workloadDays));
			taskOwnerHelper.BeginUpdate();
			foreach (var target in forecastingTargets)
			{
				foreach (var workloadDay in workloadDays)
				{
					if (workloadDay.OpenForWork.IsOpen && target.CurrentDate == workloadDay.CurrentDate)
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