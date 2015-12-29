using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDataWithToFewDaysOffTest
	{
		private MockRepository _mocks;
		private IMatrixDataWithToFewDaysOff _target;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IMatrixData _matrixData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_target = new MatrixDataWithToFewDaysOff(_dayOffsInPeriodCalculator);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_matrixData = _mocks.StrictMock<IMatrixData>();
		}

		[Test]
		public void ShouldReturnMatrixDataIfNotEnoughDaysOff()
		{
			var scheduleDay1  = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			using (_mocks.Record())
			{
				Expect.Call(_matrixData.Matrix).Return(_matrix);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				int targetDaysOff;
				//int dayOffsNow;
				IList<IScheduleDay> dayOffsNow = new List<IScheduleDay>{scheduleDay1, scheduleDay2};
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out targetDaysOff, out dayOffsNow)).Return(false).OutRef(3, dayOffsNow);
			}
			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.FindMatrixesWithToFewDaysOff(new List<IMatrixData> {_matrixData});
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ShouldNotReturnMatrixDataIfNotEnoughDaysOffButHasCorrectNumberOfDaysOffDueToFlexibilityWhenFixedDayWorkTime()
		{
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var contract = _mocks.StrictMock<IContract>();

			using (_mocks.Record())
			{
				Expect.Call(_matrixData.Matrix).Return(_matrix).Repeat.AtLeastOnce();
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.Contract).Return(contract);
				Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
				int targetDaysOff;
				IList<IScheduleDay> dayOffsNow = new List<IScheduleDay> { scheduleDay1, scheduleDay2 };
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out targetDaysOff, out dayOffsNow)).Return(true).OutRef(3, dayOffsNow);
			}
			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { _matrixData });
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldReturnMatrixDataIfNotEnoughDaysOffEvenWithFlexibilityOnDaysOff()
		{
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var contract = _mocks.StrictMock<IContract>();

			using (_mocks.Record())
			{
				Expect.Call(_matrixData.Matrix).Return(_matrix).Repeat.AtLeastOnce();
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.Contract).Return(contract);
				Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
				int targetDaysOff;
				//int dayOffsNow;
				IList<IScheduleDay> dayOffsNow = new List<IScheduleDay> { scheduleDay1, scheduleDay2 };
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out targetDaysOff, out dayOffsNow)).Return(true).OutRef(3, dayOffsNow);
			}
			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { _matrixData });
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ShouldNotReturnMatrixDataIfTooManyDaysOff()
		{
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
			var contract = _mocks.StrictMock<IContract>();

			using (_mocks.Record())
			{
				Expect.Call(_matrixData.Matrix).Return(_matrix).Repeat.AtLeastOnce();
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.Contract).Return(contract);
				Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
				int targetDaysOff;
				//int dayOffsNow;
				IList<IScheduleDay> dayOffsNow = new List<IScheduleDay> {scheduleDay1, scheduleDay2, scheduleDay3};
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out targetDaysOff, out dayOffsNow)).Return(false).OutRef(2, dayOffsNow);
			}
			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { _matrixData });
				Assert.AreEqual(0, result.Count);
			}
		}
	}
}