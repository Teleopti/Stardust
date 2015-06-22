using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	[TestFixture]
	public class ForecastMethodProviderTest
	{
		[Test]
		public void ShouldCalculateMethodsWhenLessThan2Months()
		{
			var target = new ForecastMethodProvider(new LinearRegressionTrendCalculator());

			var result = target.Calculate(new DateOnlyPeriod(2014, 1, 1, 2014, 2, 10));
			result[0].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicShortTerm);
		}
	}
}