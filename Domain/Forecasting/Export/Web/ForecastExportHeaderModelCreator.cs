using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public static class ForecastExportHeaderModelCreator
	{
		public static ForecastExportHeaderModel Load(ISkill skill, IList<ISkillDay> skillDays, DateOnlyPeriod period)
		{
			var model = new ForecastExportHeaderModel
			{
				Period = period,
				SkillName = skill.Name,
				SkillTimeZoneName = skill.TimeZone.DisplayName
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