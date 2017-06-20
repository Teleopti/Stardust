using System;
using System.Collections.Generic;
using System.Linq;
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

		public ForecastExportModelCreator(
			ISkillDayLoadHelper skillDayLoadHelper,
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
			var skill = _skillRepository.Get(skillId);
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new List<ISkill>() { skill }, scenario);

			var skillDays = skillDaysBySkills[skill].ToList();
			return new ForecastExportModel
			{
				Header = ForecastExportHeaderModelCreator.Load(skill, skillDays, period),
				DailyModelForecast = ForecastExportDailyModelCreator.Load(skillDays)
			};
		}
	}
}