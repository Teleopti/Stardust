using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;

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

		protected override void Assert(SkillAccuracy measurementResult)
		{
			measurementResult.Workloads.First().Id.Should().Be.EqualTo(Workload.Id.Value);
			measurementResult.Workloads.First().Accuracies.First().MethodId.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassic);
			measurementResult.Workloads.First().Accuracies.First().MeasureResult.First().Tasks.Should().Be.EqualTo(9);
			measurementResult.Workloads.First().Accuracies.First().MeasureResult.Count().Should().Be.EqualTo(366);
			measurementResult.Workloads.First().Accuracies.First().Number.Should().Be.EqualTo(100 - Math.Round((11d - 9d)/11d*100, 1));
			measurementResult.Workloads.First().Accuracies.Second().MethodId.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicWithTrend);
			measurementResult.Workloads.First().Accuracies.Second().Number.Should().Be.EqualTo(Math.Round(100 -((11d - (9d + 1*HistoricalPeriodForForecast.EndDate.Subtract(LinearTrend.StartDate).Days + 2 - 9d)))/11d*100, 1));
			measurementResult.Workloads.First().Accuracies.First().MeasureResult.Second().Tasks.Should().Be.EqualTo(9);
			measurementResult.Workloads.First().Accuracies.First().MeasureResult.Count().Should().Be.EqualTo(366);
		}
	}
}