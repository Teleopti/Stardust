using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class SkillDayAlreadyExistTest : QuickForecastTest
	{
		private const int newExpectedNumberOfTasks = 48;

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask {Interval = startDateOnHistoricalPeriod, StatOfferedTasks = newExpectedNumberOfTasks}
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