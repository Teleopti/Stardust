using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class MeasurementShouldReturnCorrectlyTest : MeasureForecastTest
	{
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
			var statisticTasks2 = new StatisticTask { Interval = HistoricalPeriod.EndDate.Date, StatOfferedTasks = 11 };
			return new[] {statisticTasks1, statisticTasks2};
		}

		protected override void Assert(ForecastingAccuracy[] measurementResult)
		{
			measurementResult.First().WorkloadId.Should().Be.EqualTo(Workload.Id.Value);
			measurementResult.First().Accuracy.Should().Be.EqualTo(100 - Math.Round((11d - 9d)/11d*100, 1));
		}
	}
}