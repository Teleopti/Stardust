using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NoStatisticNorValidatedTest : QuickForecastTest
	{
		protected override void Assert(ForecastModel forecastResult)
		{
			forecastResult.ForecastDays.Should().Be.Empty();
		}
	}
}