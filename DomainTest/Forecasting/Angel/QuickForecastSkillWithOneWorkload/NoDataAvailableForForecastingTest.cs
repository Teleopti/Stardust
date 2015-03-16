using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoDataAvailableForForecastingTest : MeasureForecastTest
	{
		protected override void Assert(ForecastingAccuracy[] measurementResult)
		{
			measurementResult.First().CanForecast.Should().Be.EqualTo(false);
			measurementResult.First().Accuracy.Should().Be.EqualTo(Double.NaN);
		}
	}
}