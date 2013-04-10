using System;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	public class ForecastConfigurable : IDataSetup
	{
		public string Skill { get; set; }
		public DateTime Date { get; set; }
		public int Hours { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var skillRepository = new SkillRepository(uow);
			var skill = skillRepository.LoadAll().Single(x => x.Name == Skill);
			if (skill.WorkloadCollection.IsEmpty())
			{
				var workload = WorkloadFactory.CreateWorkload(Skill, skill);
				var workloadRepository = new WorkloadRepository(uow);
				workloadRepository.Add(workload);

				skill.AddWorkload(workload);
			}

			var scenarioRepository = new ScenarioRepository(uow);
			var defaultScenario = scenarioRepository.LoadAll().FirstOrDefault();

			var date = new DateOnly(Date);
			var skillDayRepository = new SkillDayRepository(uow);
			var skillDays = skillDayRepository.GetAllSkillDays(new DateOnlyPeriod(date, date), new Collection<ISkillDay>(), skill, defaultScenario, true);
			skillDays.First().WorkloadDayCollection[0].MakeOpen24Hours();
			skillDays.First().WorkloadDayCollection[0].Tasks = Hours*4;
			skillDays.First().WorkloadDayCollection[0].AverageTaskTime = TimeSpan.FromMinutes(15);

		}
	}
}