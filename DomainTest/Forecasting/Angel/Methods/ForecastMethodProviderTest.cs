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
			result.Length.Should().Be.EqualTo(1);
			result[0].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicShortTerm);
		}

		[Test]
		public void ShouldCalculateMethodsWhenBeteen2Months_1YearAnd1Month()
		{
			var target = new ForecastMethodProvider(new LinearRegressionTrendCalculator());

			var result = target.Calculate(new DateOnlyPeriod(2014, 1, 1, 2014, 3, 2));
			result.Length.Should().Be.EqualTo(2);
			result[0].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTerm);
			result[1].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth);
		}

		[Test]
		public void ShouldCalculateMethodsWhenBeteen1YearAnd1Month_2Years()
		{
			var target = new ForecastMethodProvider(new LinearRegressionTrendCalculator());

			var result = target.Calculate(new DateOnlyPeriod(2014, 1, 1, 2015, 2, 2));
			result.Length.Should().Be.EqualTo(4);
			result[0].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTerm);
			result[1].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTermWithTrend);
			result[2].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth);
			result[3].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonthWithTrend);
		}

		[Test]
		public void ShouldCalculateMethodsWhenMoreThan2Years()
		{
			var target = new ForecastMethodProvider(new LinearRegressionTrendCalculator());

			var result = target.Calculate(new DateOnlyPeriod(2014, 1, 1, 2016, 1, 2));
			result.Length.Should().Be.EqualTo(4);
			result[0].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicLongTerm);
			result[1].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicLongTermWithTrend);
			result[2].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicLongTermWithDayInMonth);
			result[3].Id.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicLongTermWithDayInMonthWithTrend);
		}
	}
}