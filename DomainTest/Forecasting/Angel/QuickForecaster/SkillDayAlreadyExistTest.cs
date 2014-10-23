using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecaster
{
	public class SkillDayAlreadyExistTest : QuickForecastTest
	{
		private const int newExpectedNumberOfTasks = 48;

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(HistoricalPeriod.StartDate, Workload, new TimePeriod[] { });
			yield return new ValidatedVolumeDay(Workload, HistoricalPeriod.StartDate)
			{
				TaskOwner = historialWorkloadDay,
				ValidatedTasks = newExpectedNumberOfTasks
			};
		}

		protected override ICollection<ISkillDay> CurrentSkillDays()
		{
			const int currentNumberOfTasks = newExpectedNumberOfTasks*2;

			var futureWorkloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(Workload, FuturePeriod.StartDate);
			futureWorkloadDay.Tasks = currentNumberOfTasks;
			var skillDay = new SkillDay(
				FuturePeriod.StartDate,
				Workload.Skill,
				DefaultScenario,
				new[] {futureWorkloadDay},
				Enumerable.Empty<ISkillDataPeriod>());
			new SkillDayCalculator(skillDay.Skill, new[] {skillDay}, FuturePeriod);

			skillDay.Tasks.Should().Be.EqualTo(currentNumberOfTasks);

			return new[]{skillDay};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			Convert.ToInt32(modifiedSkillDays.Single().Tasks)
				.Should().Be.EqualTo(newExpectedNumberOfTasks);
		} 
	}
}