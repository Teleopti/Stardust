using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoDataAvailableForForecastingTest : MeasureForecastTest
	{
		protected override DateOnlyPeriod HistoricalPeriodForMeasurement
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1); }
		}
		protected override void Assert(SkillAccuracy measurementResult)
		{
			measurementResult.Workloads.First().Accuracies.Should().Be.Empty();
		}
	}
}