using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class AggregateAverageHandlingTimesForOneDayWithNoAnsweredCallsTest : QuickForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var startDateOnHistoricalPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			return new[]
			{
				new StatisticTask
				{
					Interval = startDateOnHistoricalPeriod,
					StatAnsweredTasks = 0,
					StatAverageTaskTimeSeconds = 30,
					StatAverageAfterTaskTimeSeconds = 60
				},
				new StatisticTask
				{
					Interval = startDateOnHistoricalPeriod.AddMinutes(15),
					StatAnsweredTasks = 0,
					StatAverageTaskTimeSeconds = 60,
					StatAverageAfterTaskTimeSeconds = 120
				}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var skillDay = modifiedSkillDays.Single();
			skillDay.AverageTaskTime.TotalSeconds
				.Should().Be.EqualTo(45);
			skillDay.AverageAfterTaskTime.TotalSeconds
				.Should().Be.EqualTo(90);
		}
	}
}