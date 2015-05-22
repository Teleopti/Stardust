using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class MeasurementShouldPickTheBestMethodTest : MeasureForecastTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var statisticTasks1 = new StatisticTask { Interval = HistoricalPeriodForForecast.StartDate.Date, StatOfferedTasks = 9 };
			var statisticTasks2 = new StatisticTask { Interval = HistoricalPeriodForForecast.EndDate.Date, StatOfferedTasks = 11 };
			return new[] { statisticTasks1, statisticTasks2 };
		}

		protected override void Assert(WorkloadAccuracy measurementResult)
		{
			measurementResult.Id.Should().Be.EqualTo(Workload.Id.Value);
			measurementResult.Accuracies.First().IsSelected.Should().Be.True();
			measurementResult.Accuracies.Second().IsSelected.Should().Be.False();
		}
	}
}