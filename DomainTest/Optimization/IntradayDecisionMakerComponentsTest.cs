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

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			var converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			var extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_target = new IntradayDecisionMakerComponents(converter, extractor);
		}

		[Test]
		public void ShouldHaveScheduleMatrixLockableBitArrayConverter()
		{
			var converter = _target.MatrixConverter;
			Assert.That(converter.GetType(), Is.EqualTo(typeof (ScheduleMatrixLockableBitArrayConverter)));
		}

		[Test]
		public void ShouldHaveRelativeDailyValueByPersonalSkillsExtractor()
		{
			var extractor = _target.DataExtractor;
			Assert.That(extractor.GetType(), Is.EqualTo(typeof (RelativeDailyValueByPersonalSkillsExtractor)));
		}
	}
}
