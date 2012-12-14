using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDatasWithToFewDaysOffTest
	{
		private MockRepository _mocks;
		private IMatrixDatasWithToFewDaysOff _target;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IMatrixData _matrixData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_target = new MatrixDatasWithToFewDaysOff(_dayOffsInPeriodCalculator);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_matrixData = _mocks.StrictMock<IMatrixData>();
		}

		[Test]
		public void ShouldReturnMatrixDatasIfNotEnoughDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrixData.Matrix).Return(_matrix);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				int x;
				int y;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(false);
			}
			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.FindMatrixesWithToFewDaysOff(new List<IMatrixData> {_matrixData});
				Assert.AreSame(_matrix, result[0].Matrix);
			}
		}
	}
}