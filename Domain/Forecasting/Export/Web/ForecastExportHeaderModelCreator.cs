using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportHeaderModelCreator
	{
		public static ForecastExportHeaderModel Load(string scenario, IWorkload workload, IList<ISkillDay> skillDays, DateOnlyPeriod period)
		{
			var model = new ForecastExportHeaderModel
			{
				Period = period,
				SkillName = WorkloadNameBuilder.GetWorkloadName(workload.Skill.Name, workload.Name),
				SkillTimeZoneName = workload.Skill.TimeZone.DisplayName,
				Scenario = scenario
			};

			if (!skillDays.Any())
				return model;

			var firstSkillDaysDataPeriodFirst = skillDays.First().SkillDataPeriodCollection.First();
			model.ServiceLevelPercent = firstSkillDaysDataPeriodFirst.ServiceLevelPercent;
			model.ServiceLevelSeconds = firstSkillDaysDataPeriodFirst.ServiceLevelSeconds;
			model.ShrinkagePercent = firstSkillDaysDataPeriodFirst.Shrinkage;
			
			return model;
		}
	}
}