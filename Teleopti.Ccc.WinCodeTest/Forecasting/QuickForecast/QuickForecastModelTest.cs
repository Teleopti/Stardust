using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[TestFixture]
	public class QuickForecastModelTest
	{
		private QuickForecastModel _target;

		[SetUp]
		public void Setup()
		{
			_target = new QuickForecastModel();
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