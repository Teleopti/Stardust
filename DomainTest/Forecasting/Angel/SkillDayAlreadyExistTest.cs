using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class SkillDayAlreadyExistTest : QuickForecastTest
	{
		private const int newExpectedNumberOfTasks = 48;

		protected override IEnumerable<DailyStatistic> DailyStatistics()
		{
			return Enumerable.Empty<DailyStatistic>();
		}

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(HistoricalPeriod.StartDate, Workload, new TimePeriod[] { });
			return new[]
			{
				new ValidatedVolumeDay(Workload, HistoricalPeriod.StartDate)
				{
					TaskOwner = historialWorkloadDay,
					ValidatedTasks = newExpectedNumberOfTasks
				}
			};
		}

		protected override IEnumerable<ISkillDay> CurrentSkillDays()
		{
			const int currentNumberOfTasks = newExpectedNumberOfTasks*2;

			var futureWorkloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(Workload, FuturePeriod.StartDate);
			futureWorkloadDay.Tasks = currentNumberOfTasks;
			var skillDay = new SkillDay(
				FuturePeriod.StartDate,
				Workload.Skill,
				new Scenario("sdfdsf"),
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