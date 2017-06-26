using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportIntervalModelCreator
	{
		public static IList<ForecastExportIntervalModel> Load(IWorkload workload, List<ISkillDay> skillDays)
		{
			var intervalModels = new List<ForecastExportIntervalModel>();

			foreach (var skillDay in skillDays)
			{
				var workloadDay = skillDay.WorkloadDayCollection.FirstOrDefault(w => w.Workload.Id.Value == workload.Id.Value);
				if (workloadDay == null)
					continue;
				var skillTimeZone = skillDay.Skill.TimeZone;
				foreach (var taskPeriod in workloadDay.OpenTaskPeriodList)
				{
					intervalModels.Add(new ForecastExportIntervalModel
					{
						IntervalStart = TimeZoneHelper.ConvertFromUtc(taskPeriod.Period.StartDateTime, skillTimeZone),
						Calls = taskPeriod.TotalTasks,
						AverageTalkTime = taskPeriod.TotalAverageTaskTime.TotalSeconds,
						AverageAfterCallWork = taskPeriod.TotalAverageAfterTaskTime.TotalSeconds,
						AverageHandleTime = taskPeriod.TotalAverageTaskTime.TotalSeconds + taskPeriod.TotalAverageAfterTaskTime.TotalSeconds,});
				}

				foreach (var staffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					var intervalModel = intervalModels.FirstOrDefault(i => i.IntervalStart == TimeZoneHelper.ConvertFromUtc(staffPeriod.Period.StartDateTime, skillTimeZone));
					if (intervalModel == null)
						continue;

					intervalModel.Agents = staffPeriod.FStaff;
					staffPeriod.Payload.UseShrinkage = true;
					intervalModel.AgentsShrinkage = staffPeriod.FStaff;
				}
			}

			return intervalModels;
		}
	}
}