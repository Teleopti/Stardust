using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportDailyModelCreator
	{
		public static IList<ForecastExportDailyModel> Load(IList<ISkillDay> skillDays)
		{
			var dailyModel = new List<ForecastExportDailyModel>();
			foreach (var skillDay in skillDays)
			{
				if (!skillDay.OpenForWork.IsOpen)
					continue;
				dailyModel.Add(new ForecastExportDailyModel()
				{
					ForecastDate = skillDay.CurrentDate.Date,
					OpenHours = skillDay.OpenHours().First(),
					Calls = skillDay.Tasks,
					AverageTalkTime = skillDay.TotalAverageTaskTime.Seconds,
					AverageAfterCallWork = skillDay.TotalAverageAfterTaskTime.Seconds,
					AverageHandleTime = skillDay.TotalAverageTaskTime.Seconds + skillDay.TotalAverageAfterTaskTime.Seconds,
					ForecastedHours = skillDay.ForecastedDistributedDemand.TotalHours,
					ForecastedHoursShrinkage = skillDay.ForecastedDistributedDemandWithShrinkage.TotalHours
				});
			}

			return dailyModel;
		}
	}
}
