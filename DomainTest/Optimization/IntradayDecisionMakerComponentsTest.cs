using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class IntradayDecisionMakerComponentsTest
	{
		private IntradayDecisionMakerComponents _target;
		private MockRepository _mocks;
		private IScheduleMatrixLockableBitArrayConverter _converter;
		private IScheduleResultDataExtractor _extractor;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			_extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_target = new IntradayDecisionMakerComponents(_converter, _extractor);
		}

		[Test]
		public void ShouldHaveScheduleMatrixLockableBitArrayConverter()
		{
			Assert.That(_target.MatrixConverter, Is.EqualTo(_converter));
		}

		[Test]
		public void ShouldHaveRelativeDailyValueByPersonalSkillsExtractor()
		{
			Assert.That(_target.DataExtractor, Is.EqualTo(_extractor));
		}
	}
}
