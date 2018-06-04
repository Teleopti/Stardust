﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class OnlyStatisticsTest : QuickForecastTest
	{
		private const int expectedNumberOfTasks = 17;

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var dateTimeOnStartPeriod = HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()).StartDateTime.AddHours(12);
			yield return new StatisticTask {Interval = dateTimeOnStartPeriod, StatOfferedTasks = expectedNumberOfTasks};
		}

		protected override void Assert(ForecastModel forecastResult)
		{
			Convert.ToInt32(forecastResult.ForecastDays.Single().Tasks)
				.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}