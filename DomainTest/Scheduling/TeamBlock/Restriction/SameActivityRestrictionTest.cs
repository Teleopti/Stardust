using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class SameActivityRestrictionTest
	{
		private IActivity _activity;
		private SameActivityRestriction _target;
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay1;
		private IScheduleDayPro _scheduleDayPro;
		private DateTimePeriod _period;
		private IPersonAssignment _personAssignment;
		private IScheduleDay _scheduleDay2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_activity = ActivityFactory.CreateActivity("activity");
			_target = new SameActivityRestriction(_activity);
			_dateOnly = new DateOnly(2012, 12, 7);
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldExtractSameActivityRestrictionFromScheduleDay()
		{
			var dateList = new List<DateOnly>{_dateOnly};
			var sameActivity = new CommonActivity {Activity = _activity, Periods = new List<DateTimePeriod> {_period}};
			var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var shiftLayers = new List<MainShiftLayer>{new MainShiftLayer(_activity, _period)};
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro);
				Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftLayers).Return(shiftLayers);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				{
					CommonActivity = sameActivity
				};

				var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractNullRestrictionWhenANewDayHasDifferentActivities()
		{
			var dateList = new List<DateOnly> { _dateOnly };
			var sameActivity = new CommonActivity { Activity = _activity, Periods = new List<DateTimePeriod> { _period } };
			var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var shiftLayers = new List<MainShiftLayer> { new MainShiftLayer(ActivityFactory.CreateActivity("new"), _period) };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro);
				Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftLayers).Return(shiftLayers);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				{
					CommonActivity = sameActivity
				};

				var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.Not.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractNullRestrictionWhenTwoDaysHasDifferentActivities()
		{
			var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
			
			var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var shiftLayers1 = new List<MainShiftLayer> { new MainShiftLayer(ActivityFactory.CreateActivity("new1"), _period) };
			var shiftLayers2 = new List<MainShiftLayer> { new MainShiftLayer(ActivityFactory.CreateActivity("new2"), _period) };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro);
				Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftLayers).Return(shiftLayers1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(scheduleDayPro);
				Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftLayers).Return(shiftLayers2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

				var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.EqualTo(expected));
			}
		}


		[Test]
		public void ShouldExtractSameActivityRestrictionWhenPersonAssignmentIsNull()
		{
			var dateList = new List<DateOnly> { _dateOnly };
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
				var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractSameActivityRestrictionWhenScheduleIsNull()
		{
			var dateList = new List<DateOnly> { _dateOnly };
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(null);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
				var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.EqualTo(expected));
			}
		}
	}
}
