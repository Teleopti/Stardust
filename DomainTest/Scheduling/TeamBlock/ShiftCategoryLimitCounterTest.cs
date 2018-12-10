using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class ShiftCategoryLimitCounterTest
	{
		private ShiftCategoryLimitCounter _target;
		private IShiftCategoryLimitation _shiftCategoryLimitation;
		private IShiftCategory _shiftCategory;
		private MockRepository _mock;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDayPro _scheduleDayPro3;
		private IScheduleDayPro _scheduleDayPro4;
		private IScheduleDayPro _scheduleDayPro5;
		private IScheduleDayPro _scheduleDayPro6;
		private IScheduleDayPro _scheduleDayPro7;
		private ITeamInfo _teamInfo;
		private DateOnly _dateOnly;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros;
		private IScheduleDayPro[] _scheduleDayPros;
		private IScheduleDay _scheduleDay;
		private IPersonAssignment _personAssignment;
			
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_target = new ShiftCategoryLimitCounter();
			_shiftCategory = new ShiftCategory("shiftCategory");
			_shiftCategory.SetId(Guid.NewGuid());
			_shiftCategoryLimitation = new ShiftCategoryLimitation(_shiftCategory);
			_dateOnly = new DateOnly(2015, 1, 1);
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro4 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro5 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro6 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro7 = _mock.StrictMock<IScheduleDayPro>();	
			_scheduleDayPros = new []{_scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5, _scheduleDayPro6, _scheduleDayPro7};
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
		}

		[Test]
		public void ShouldReturnTruewWhenOverWeekLimit()
		{
			_shiftCategoryLimitation.Weekly = true;
			_shiftCategoryLimitation.MaxNumberOf = 3;

			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_dateOnly)).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_scheduleDay);

				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalsewWhenUnderWeekLimit()
		{
			_shiftCategoryLimitation.Weekly = true;
			_shiftCategoryLimitation.MaxNumberOf = 8;

			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_dateOnly)).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_scheduleDay);

				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueWhenOverPeriodLimit()
		{
			_shiftCategoryLimitation.Weekly = false;
			_shiftCategoryLimitation.MaxNumberOf = 2;

			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_scheduleDay);

				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenUnderPeriodLimit()
		{
			_shiftCategoryLimitation.Weekly = false;
			_shiftCategoryLimitation.MaxNumberOf = 8;

			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_scheduleDayPros);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_scheduleDay);

				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HaveMaxOfShiftCategory(_shiftCategoryLimitation, _teamInfo, _dateOnly);
				Assert.IsFalse(result);
			}
		}
	}
}
