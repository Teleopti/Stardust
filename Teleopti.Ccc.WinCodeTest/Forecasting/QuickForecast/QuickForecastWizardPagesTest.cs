﻿using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class QuickForecastWizardPagesTest
	{
		private QuickForecastWizardPages _target;
		private QuickForecastCommandDto _model;

		[SetUp]
		public void Setup()
		{
			_model = new QuickForecastCommandDto();
			_target = new QuickForecastWizardPages(_model);
		}

		[Test]
		public void ShouldHaveACommand()
		{
			Assert.That(_target.CreateNewStateObj(),Is.Not.Null);
			Assert.That(_target.WindowText, Is.Not.Empty);
			Assert.That(_target.Name, Is.Not.Empty);
			Assert.That(_target.CreateNewStateObj(),Is.EqualTo(_model));
		}
	}
}