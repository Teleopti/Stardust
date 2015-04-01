using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class NoDataAvailableForMeasurementTest : MeasureForecastTest
	{
		protected override void Assert(SkillAccuracy measurementResult)
		{
			measurementResult.Workloads.First().Accuracies.Should().Be.Empty();
		}

		protected override DateOnlyPeriod HistoricalPeriod
		{
			get
			{
				var date = new DateTime(2000, 1, 1);
				return new DateOnlyPeriod(new DateOnly(date.AddYears(-1)), new DateOnly(date));
			}
		}

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var statisticTasks1 = new StatisticTask { Interval = HistoricalPeriod.StartDate.Date, StatOfferedTasks = 9 };
			return new[] { statisticTasks1 };
		}
	}
}