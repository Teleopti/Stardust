using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class MatrixMultipleShiftsLockerTest
	{
		private MatrixMultipleShiftsLocker _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrix;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private DateOnly _dateOnly1;
		private DateOnly _dateOnly2;
		private IList<IScheduleMatrixPro> _scheduleMatrixList;
		private IPersonAssignment _personAssignment1;
		private IPersonAssignment _personAssignment2;
		private IList<IPersonAssignment> _multiplePersonAssignments;
		private IList<IPersonAssignment> _singlePersonAssignment;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_scheduleMatrixList = new List<IScheduleMatrixPro> { _scheduleMatrix };
			_dateOnly1 = new DateOnly(2012, 1, 1);
			_dateOnly2 = new DateOnly(2012, 1, 2);
			_personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			_personAssignment2 = _mocks.StrictMock<IPersonAssignment>();
			_multiplePersonAssignments = new List<IPersonAssignment> { _personAssignment1, _personAssignment2 };
			_singlePersonAssignment = new List<IPersonAssignment>{_personAssignment1};
			_target = new MatrixMultipleShiftsLocker(_scheduleMatrixList);
		}

		[Test]
		public void ShouldLockDaysWithMultipleShifts()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 }));
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly2);
				Expect.Call(_scheduleDay1.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(_singlePersonAssignment));
				Expect.Call(_scheduleDay2.PersonAssignmentCollection()).Return(new ReadOnlyCollection<IPersonAssignment>(_multiplePersonAssignments));
				Expect.Call(() => _scheduleMatrix.LockPeriod( new DateOnlyPeriod(_dateOnly2, _dateOnly2)));	
			}

			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}
	}
}
