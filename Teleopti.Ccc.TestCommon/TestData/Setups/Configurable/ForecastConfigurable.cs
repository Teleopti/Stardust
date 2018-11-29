using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ForecastConfigurable : IDataSetup
	{
		private readonly string _skillName;
		private readonly DateTime _date;
		private readonly bool _opened24Hours;

		public ForecastConfigurable(string skillName, DateTime date, bool opened24Hours = false)
		{
			_skillName = skillName;
			_date = date;
			_opened24Hours = opened24Hours;
		}
		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var openHours = _opened24Hours ? new TimePeriod(0, 0, 23, 59) : new TimePeriod(8, 17);
			var hours = openHours.SpanningTime().Hours;
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name == _skillName);
			
			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var defaultScenario = scenarioRepository.LoadAll().FirstOrDefault();
			defaultScenario.EnableReporting = true;

			var date = new DateOnly(_date);
			var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(defaultScenario, date, 5, new Tuple<TimePeriod, double>(openHours, 5));
			skillDayRepository.Add(skillDay);
			var workloadDay = skillDay.WorkloadDayCollection[0];

			workloadDay.ChangeOpenHours(new List<TimePeriod> { openHours });
			workloadDay.Tasks = hours*50;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(90);
		}
	}
}