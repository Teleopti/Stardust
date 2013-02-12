using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[TestFixture]
	public class QuickForecastModelTest
	{
		private MockRepository _mocks;
		private QuickForecastModel _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new QuickForecastModel();
		}

		[Test]
		public void ShouldHaveDefaults()
		{
			Assert.That(_target.StatisticPeriod, Is.Not.Null);
			Assert.That(_target.TemplatePeriod, Is.Not.Null);
			Assert.That(_target.TargetPeriod, Is.Not.Null);
		}
	}

}