using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class BothStatisticsAndValidatedTest : QuickForecastTest
	{
		private const int expectedNumberOfTasks = 123;

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			const int nonExpectedNumberOfTasks = expectedNumberOfTasks + 1;
			var dateTimeOnStartPeriod = HistoricalPeriod.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			yield return new StatisticTask {Interval = dateTimeOnStartPeriod, StatOfferedTasks = nonExpectedNumberOfTasks};
		}

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(HistoricalPeriod.StartDate, Workload, new TimePeriod[] { });
			yield return new ValidatedVolumeDay(Workload, HistoricalPeriod.StartDate)
			{
				TaskOwner = historialWorkloadDay,
				ValidatedTasks = expectedNumberOfTasks
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			Convert.ToInt32(modifiedSkillDays.Single().Tasks)
				.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}