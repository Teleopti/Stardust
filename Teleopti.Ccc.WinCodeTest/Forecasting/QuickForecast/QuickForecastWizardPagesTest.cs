using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.QuickForecast
{
	[TestFixture]
	public class QuickForecastWizardPagesTest
	{
		private MockRepository _mocks;
		private QuickForecastWizardPages _target;
		private QuickForecastModel _model;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = new QuickForecastModel();
			_target = new QuickForecastWizardPages(_model);

		}


	}

}