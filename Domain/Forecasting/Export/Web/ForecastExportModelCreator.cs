using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportModelCreator
	{
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillRepository _skillRepository;

		public ForecastExportModelCreator(ISkillDayLoadHelper skillDayLoadHelper, 
			IScenarioRepository scenarioRepository,
			ISkillRepository skillRepository)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
			_scenarioRepository = scenarioRepository;
			_skillRepository = skillRepository;
		}
		public ForecastExportModel Load(Guid skillId, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var skill = _skillRepository.LoadSkills(new List<Guid>() { skillId }).First();
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new List<ISkill>(){skill}, scenario);
			if (!skillDaysBySkills[skill].Any())
				return new ForecastExportModel();
			var DailyModel = new List<DailyModelForecast>();
			foreach (var skillDay in skillDaysBySkills[skill])
			{
				DailyModel.Add(new DailyModelForecast()
				{
					ForecastDate = skillDay.CurrentDate.Date,
					OpenHours = skillDay.OpenHours().First(),
					Calls = skillDay.Tasks,
					AverageTalkTime = skillDay.TotalAverageTaskTime.Seconds,
					AfterCallWork = skillDay.TotalAverageAfterTaskTime.Seconds,
					AverageHandleTime = skillDay.TotalAverageTaskTime.Seconds + skillDay.AverageAfterTaskTime.Seconds,
					ForecastedHours = skillDay.SkillStaffPeriodCollection.Sum(x => x.FStaffHours()),
					ForecastedHoursShrinkage = skillDay.SkillStaffPeriodCollection.Sum(x => x.FStaffHours())
				});
			}

			return new ForecastExportModel
			{
				Header = new ForecastExportHeader
				{
					Period = period,
					SkillName = skill.Name,
					SkillTimeZoneName = skill.TimeZone.DisplayName,
					ServiceLevelPercent = skillDaysBySkills[skill].First().SkillDataPeriodCollection.First().ServiceLevelPercent,
					ServiceLevelSeconds = skillDaysBySkills[skill].First().SkillDataPeriodCollection.First().ServiceLevelSeconds,
					ShrinkagePercent = skillDaysBySkills[skill].First().SkillDataPeriodCollection.First().Shrinkage,
					DailyModelForecast = DailyModel
				}
			};
		}
	}
}
