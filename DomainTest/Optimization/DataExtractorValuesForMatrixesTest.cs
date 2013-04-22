using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class DataExtractorValuesForMatrixesTest
	{
		private IDataExtractorValuesForMatrixes _target;
		private MockRepository _mocks;
	    private IScheduleResultDataExtractor _extractor;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_target = new DataExtractorValuesForMatrixes();
		}

		[Test]
		public void ShouldHoldLockableData()
		{
            _target.Add(_matrix, _extractor);

			Assert.That(_target.Data.Count, Is.EqualTo(1));
		}
	}
}
