using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[TestFixture]
	public class QuickForecastModelTest
	{
		private QuickForecastCommandDto _target;

		[SetUp]
		public void Setup()
		{
			_target = new QuickForecastCommandDto();
		}

		[Test]
		public void ShouldHaveDefaults()
		{
			Assert.That(_target.StatisticPeriod, Is.Not.Null);
			Assert.That(_target.TemplatePeriod, Is.Not.Null);
			Assert.That(_target.TargetPeriod, Is.Not.Null);
			Assert.That(_target.SmoothingStyle, Is.EqualTo(5));
		}
	}

}