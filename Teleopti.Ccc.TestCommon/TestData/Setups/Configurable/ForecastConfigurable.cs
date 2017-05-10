using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ForecastConfigurable : IDataSetup
	{
		private readonly string _skillName;
		private readonly DateTime _date;

		public ForecastConfigurable(string skillName, DateTime date)
		{
			_skillName = skillName;
			_date = date;
		}
		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var openHours = new TimePeriod(8, 17);
			var hours = openHours.SpanningTime().Hours;
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name == _skillName);
			
			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var defaultScenario = scenarioRepository.LoadAll().FirstOrDefault();
			defaultScenario.EnableReporting = true;

			var date = new DateOnly(_date);
			var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(defaultScenario, date, 5, ServiceAgreement.DefaultValues(),
				new Tuple<TimePeriod, double>(openHours, 5));
			skillDayRepository.Add(skillDay);
			var workloadDay = skillDay.WorkloadDayCollection[0];

			workloadDay.ChangeOpenHours(new List<TimePeriod> {openHours});
			workloadDay.Tasks = hours*50;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(90);
		}
	}
}