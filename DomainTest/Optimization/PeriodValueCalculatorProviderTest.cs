using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class PeriodValueCalculatorProviderTest
	{
		private IPeriodValueCalculatorProvider _target;
		private MockRepository _mocks;
		private IScheduleResultDataExtractor _extractor;
		private IAdvancedPreferences _advancedPreferences;

		[SetUp]
		public void Setup()
		{
			_target = new PeriodValueCalculatorProvider();
			_mocks = new MockRepository();
			_extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_advancedPreferences = new AdvancedPreferences();
		}

		[Test]
		public void ShouldReturnStandardDeviationByDefault()
		{
			IPeriodValueCalculator ret = _target.CreatePeriodValueCalculator(_advancedPreferences, _extractor);
			Assert.AreEqual(typeof(StdDevPeriodValueCalculator), ret.GetType());
		}

		[Test]
		public void ShouldReturnRmsIfAsked()
		{
			_advancedPreferences.TargetValueCalculation = TargetValueOptions.RootMeanSquare;
			IPeriodValueCalculator ret = _target.CreatePeriodValueCalculator(_advancedPreferences, _extractor);
			Assert.AreEqual(typeof(RmsPeriodValueCalculator), ret.GetType());
		}

		[Test]
		public void ShouldReturnTeleoptiIfAsked()
		{
			_advancedPreferences.TargetValueCalculation = TargetValueOptions.Teleopti;
			IPeriodValueCalculator ret = _target.CreatePeriodValueCalculator(_advancedPreferences, _extractor);
			Assert.AreEqual(typeof(TeleoptiPeriodValueCalculator), ret.GetType());
		}
	}
}