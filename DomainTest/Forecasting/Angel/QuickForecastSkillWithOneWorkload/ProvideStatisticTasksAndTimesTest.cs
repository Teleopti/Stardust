using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class ProvideStatisticTasksAndTimesTest : QuickForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask
				{
					Interval = startDateOnHistoricalPeriod, 
					StatOfferedTasks = 6,
					StatAverageTaskTimeSeconds = 35,
					StatAverageAfterTaskTimeSeconds = 50
				}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var firstDay = modifiedSkillDays.Single(x => x.CurrentDate == FuturePeriod.StartDate);
			Convert.ToInt32(firstDay.Tasks)
				.Should().Be.EqualTo(6);
			Convert.ToInt32(firstDay.AverageTaskTime.TotalSeconds)
				.Should().Be.EqualTo(35);
			Convert.ToInt32(firstDay.AverageAfterTaskTime.TotalSeconds)
				.Should().Be.EqualTo(50);
		}
	}
}