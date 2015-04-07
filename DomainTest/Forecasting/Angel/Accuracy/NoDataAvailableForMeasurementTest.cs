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

		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			var statisticTasks1 = new StatisticTask { Interval = HistoricalPeriodForMeasurement.StartDate.Date, StatOfferedTasks = 9 };
			return new[] { statisticTasks1 };
		}
	}
}