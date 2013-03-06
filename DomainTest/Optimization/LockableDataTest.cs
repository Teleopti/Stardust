using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class LockableDataTest
	{
		private ILockableData _target;
		private MockRepository _mocks;
		private IScheduleMatrixLockableBitArrayConverter _converter;
		private IScheduleResultDataExtractor _extractor;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			_extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_target = new LockableData();
		}

		[Test]
		public void ShouldHoldLockableData()
		{
			var components = new IntradayDecisionMakerComponents(_converter, _extractor);
			_target.Add(_matrix, components);

			Assert.That(_target.Data.Count, Is.EqualTo(1));
			Assert.That(_target.Data[_matrix], Is.EqualTo(components));
		}
	}
}
