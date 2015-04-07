using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class AggregateStatisticTasksForTwoDaysTest : QuickForecastTest
	{
		protected override DateOnlyPeriod HistoricalPeriodForForecast
		{
			get { return new DateOnlyPeriod(2000,1,1,2000,1,2); }
		}

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask {Interval = startDateOnHistoricalPeriod, StatOfferedTasks = 6},
				new StatisticTask {Interval = startDateOnHistoricalPeriod.AddDays(1), StatOfferedTasks = 7}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var firstDay = modifiedSkillDays.Single(x => x.CurrentDate == FuturePeriod.StartDate);
			var lastDay = modifiedSkillDays.Single(x => x.CurrentDate == FuturePeriod.StartDate.AddDays(1));
			Convert.ToInt32(firstDay.Tasks)
				.Should().Be.EqualTo(6);
			Convert.ToInt32(lastDay.Tasks)
				.Should().Be.EqualTo(7);
		}
	}
}