using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class OnlyStatisticsTest : QuickForecastTest
	{
		private const int expectedNumberOfTasks = 17;

		protected override IEnumerable<DailyStatistic> DailyStatistics()
		{
			return new[] {new DailyStatistic(HistoricalPeriod.StartDate, expectedNumberOfTasks,0,0)};
		}

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			return Enumerable.Empty<IValidatedVolumeDay>();
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			Convert.ToInt32(modifiedSkillDays.Single().Tasks)
				.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}