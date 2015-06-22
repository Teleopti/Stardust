using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class MeasurementShouldReturnCorrectlyTest : MeasureForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var statisticTasks1 = new StatisticTask { Interval = HistoricalPeriodForForecast.StartDate.Date, StatOfferedTasks = 9 };
			var statisticTasks2 = new StatisticTask { Interval = HistoricalPeriodForForecast.EndDate.Date, StatOfferedTasks = 11 };
			return new[] {statisticTasks1, statisticTasks2};
		}

		protected override void Assert(WorkloadAccuracy measurementResult)
		{
			measurementResult.Id.Should().Be.EqualTo(Workload.Id.Value);

			var one = measurementResult.Accuracies.Single(x => x.MethodId == ForecastMethodType.TeleoptiClassicLongTerm);
			one.MeasureResult.First().Tasks.Should().Be.EqualTo(9);
			one.MeasureResult.Count().Should().Be.EqualTo(365);
			one.Number.Should().Be.EqualTo(Math.Round(100 - (11d - 9d) / 11d * 100, 1));
			var another = measurementResult.Accuracies.Single(x => x.MethodId == ForecastMethodType.TeleoptiClassicLongTermWithTrend);
			another.Number.Should().Be.EqualTo(Math.Round(100 -((11d - (9d + 1*HistoricalPeriodForForecast.EndDate.Subtract(LinearTrend.StartDate).Days + 2 - 9d)))/11d*100, 1));
		}
	}
}