using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class AggregateStatisticTasksForOneDayTest : QuickForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask {Interval = startDateOnHistoricalPeriod, StatOfferedTasks = 6},
				new StatisticTask {Interval = startDateOnHistoricalPeriod.AddMinutes(15), StatOfferedTasks = 7}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var skillDay = modifiedSkillDays.Single();
			Convert.ToInt32(skillDay.Tasks)
				.Should().Be.EqualTo(13);
		}
	}
}