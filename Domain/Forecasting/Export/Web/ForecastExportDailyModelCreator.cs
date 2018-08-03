using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportDailyModelCreator
	{
		public static IList<ForecastExportDailyModel> Load(Guid workloadId, IList<ISkillDay> skillDays)
		{
			var dailyModel = new List<ForecastExportDailyModel>();
			foreach (var skillDay in skillDays)
			{
				if (!skillDay.OpenForWork.IsOpen)
					continue;
				var workload = skillDay.WorkloadDayCollection.First(x => x.Workload.Id.Value == workloadId);
				dailyModel.Add(new ForecastExportDailyModel()
				{
					ForecastDate = skillDay.CurrentDate.Date,
					OpenHours = skillDay.OpenHours().First(),
					Calls = workload.TotalTasks,
					AverageTalkTime = workload.TotalAverageTaskTime.TotalSeconds,
					AverageAfterCallWork = workload.TotalAverageAfterTaskTime.TotalSeconds,
					AverageHandleTime = workload.TotalAverageTaskTime.TotalSeconds + workload.TotalAverageAfterTaskTime.TotalSeconds,
					ForecastedHours = skillDay.ForecastedIncomingDemand.TotalHours,
					ForecastedHoursShrinkage = skillDay.ForecastedIncomingDemandWithShrinkage.TotalHours
				});
			}

			return dailyModel;
		}
	}
}
