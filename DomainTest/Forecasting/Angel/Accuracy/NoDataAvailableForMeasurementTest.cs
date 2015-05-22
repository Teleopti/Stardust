using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class NoDataAvailableForMeasurementTest : MeasureForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var statisticTasks1 = new StatisticTask { Interval = HistoricalPeriodForForecast.StartDate.Date, StatOfferedTasks = 9 };
			return new[] { statisticTasks1 };
		}

		protected override void Assert(WorkloadAccuracy measurementResult)
		{
			measurementResult.Accuracies.Should().Be.Empty();
		}
	}
}