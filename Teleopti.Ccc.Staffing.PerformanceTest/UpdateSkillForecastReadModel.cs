using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	public class UpdateSkillForecastReadModel
	{
		private readonly SkillForecastIntervalCalculator _skillForecastIntervalCalculator;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public UpdateSkillForecastReadModel(ISkillDayRepository skillDayRepository, ISkillRepository skillRepository, IScenarioRepository scenarioRepository,
			SkillForecastIntervalCalculator skillForecastIntervalCalculator)
		{
			_skillForecastIntervalCalculator = skillForecastIntervalCalculator;
			_skillDayRepository = skillDayRepository;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Update(DateTimePeriod dateTimePeriod)
		{
			var dateOnlyPeriod = dateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc);
			var skills = _skillRepository.LoadAll().ToList();

			var allSkillDays = new List<ISkillDay>();
			foreach (var skillBatch in skills.Batch(30))
			{
				var skillDays = _skillDayRepository.FindReadOnlyRange(dateOnlyPeriod,
					skillBatch, _scenarioRepository.LoadDefaultScenario()).ToList();
				allSkillDays.AddRange(skillDays);
			}

			_skillForecastIntervalCalculator.Calculate(allSkillDays, skills, dateOnlyPeriod);
		}
	}
}