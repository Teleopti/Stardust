using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoDataAvailableForForecastingTest : MeasureForecastTest
	{
		protected override DateOnlyPeriod HistoricalPeriodForForecast
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1); }
		}
		protected override void Assert(WorkloadAccuracy measurementResult)
		{
			measurementResult.Accuracies.Should().Be.Empty();
		}
	}
}