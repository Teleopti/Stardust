using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class ForecastConfigurable : IDataSetup
	{
		public string Skill { get; set; }
		public DateTime Date { get; set; }
		public int Hours { get; set; }
		public int ServiceLevelSeconds { get; set; }
		public int ServiceLevelPercentage { get; set; }
		public string OpenFrom { get; set; }
		public string OpenTo { get; set; }

		public ForecastConfigurable()
		{
			Hours = 8;
			OpenFrom = "09:00";
			OpenTo = "16:00";
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = new SkillRepository(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name == Skill);
			if (skill.WorkloadCollection.IsEmpty())
			{
				var workload = WorkloadFactory.CreateWorkload(Skill, skill);
				var workloadRepository = new WorkloadRepository(currentUnitOfWork);
				workloadRepository.Add(workload);
				skill.AddWorkload(workload);
			}

			var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
			var defaultScenario = scenarioRepository.LoadAll().FirstOrDefault();

			var date = new DateOnly(Date);
			var skillDayRepository = new SkillDayRepository(currentUnitOfWork);
			var skillDays = skillDayRepository.GetAllSkillDays(new DateOnlyPeriod(date, date), new Collection<ISkillDay>(), skill,
			                                                   defaultScenario, s => skillDayRepository.AddRange(s));
			var skillDay = skillDays.First();
			skillDay.MergeSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());
			var workloadDay = skillDay.WorkloadDayCollection[0];

			workloadDay.ChangeOpenHours(new List<TimePeriod> {new TimePeriod(TimeSpan.Parse(OpenFrom), TimeSpan.Parse(OpenTo))});
			workloadDay.Tasks = Hours*4;
			workloadDay.AverageTaskTime = TimeSpan.FromMinutes(15);

			if (ServiceLevelPercentage > 0)
				skillDay.SkillDataPeriodCollection[0].ServiceLevelPercent = new Percent(ServiceLevelPercentage / 100.0);
			if (ServiceLevelSeconds>0)
				skillDay.SkillDataPeriodCollection[0].ServiceLevelSeconds = ServiceLevelSeconds;
		}
	}
}