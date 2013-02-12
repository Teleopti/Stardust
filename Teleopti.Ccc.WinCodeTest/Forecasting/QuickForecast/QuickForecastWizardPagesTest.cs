﻿using NUnit.Framework;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[TestFixture]
	public class QuickForecastWizardPagesTest
	{
		private QuickForecastWizardPages _target;
		private QuickForecastModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new QuickForecastModel();
			_target = new QuickForecastWizardPages(_model);
		}

		[Test]
		public void ShouldHaveACommand()
		{
			Assert.That(_target.GetCommand(),Is.Not.Null);
			Assert.That(_target.WindowText, Is.Not.Empty);
			Assert.That(_target.Name, Is.Not.Empty);
			Assert.That(_target.CreateNewStateObj(),Is.EqualTo(_model));
		}
	}
}