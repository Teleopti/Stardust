﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ShiftProjectionCacheTest
	{
		private MockRepository _mocks;
		private ShiftProjectionCache target;
		private IWorkShift workShift;
		private DateOnly schedulingDate;
		private ShiftCategory category;
		private IActivity activity;
		private TimeSpan start;
		private TimeSpan end;
		private TimeZoneInfo timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			schedulingDate = new DateOnly(2009, 2, 2);
			category = new ShiftCategory("öfdöf");
			workShift = new WorkShift(category);
			activity = new Activity("alsfd");
			activity.InWorkTime = true;
			start = new TimeSpan(8, 0, 0);
			end = new TimeSpan(17, 0, 0);
			DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity, period));
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
			// blir 7 - 16 med denna tidszon (W. Central Africa Standard Time)
			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker, new DateOnlyAsDateTimePeriod(schedulingDate, timeZoneInfo));
		}

		[Test]
		public void VerifyCreateShiftProjectionCache()
		{
			Assert.IsNotNull(target);
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.IsNotNull(target.TheMainShift);
			Assert.IsNotNull(target.TheWorkShift);
			Assert.IsNotNull(target.MainShiftProjection);
			Assert.IsNotNull(target.WorkShiftProjectionPeriod);
			Assert.IsNotNull(target.WorkShiftProjectionContractTime);
		}

		[Test]
		public void WhenEmptyPersonalShiftsAndMeetingsAreInWorkTimeReturnsTrue()
		{
			var readMeetings = new IPersonMeeting[0];

			Assert.IsTrue(target.PersonalShiftsAndMeetingsAreInWorkTime(readMeetings, null));
		}

		[Test]
		public void ShouldReturnFalseIfMeetingTimeNotOk()
		{
			var personMeeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new []{personMeeting};

			using(_mocks.Record())
			{
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimeMeeting(null, null)).IgnoreArguments().Return(false);
			}

			using(_mocks.Record())
			{
				var result = target.PersonalShiftsAndMeetingsAreInWorkTime(meetings, null);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfPersonalShiftTimeNotOk()
		{
			var personAssignment = _mocks.StrictMock<IPersonAssignment>();

			using(_mocks.Record())
			{
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(null, null)).IgnoreArguments().Return(false);
			}

			using(_mocks.Playback())
			{
				var result = target.PersonalShiftsAndMeetingsAreInWorkTime(new IPersonMeeting[0], personAssignment);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnTrueIfMeetingAndPersonalShiftTimeOk()
		{
			var personAssignment = _mocks.StrictMock<IPersonAssignment>();
			var personMeeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new [] { personMeeting };

			using (_mocks.Record())
			{
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimeMeeting(null, null)).IgnoreArguments().Return(true);
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(null, null)).IgnoreArguments().Return(true);
			}

			using (_mocks.Playback())
			{
				var result = target.PersonalShiftsAndMeetingsAreInWorkTime(meetings, personAssignment);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void CheckIfPersonalShiftsAndMeetingsAreInWorkTimeReturnsFalseWhenMeetingOutsideWorkTime()
		{
			IActivity lunch = new Activity("lunch");
			lunch.InWorkTime = false;
			start = new TimeSpan(12, 0, 0);
			end = new TimeSpan(13, 0, 0);
			// gets one hour erlier in definied timezone 11 - 12
			// and the meeting 11 - 11:30 is not correct 
			DateTimePeriod lunchPeriod = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(lunch, lunchPeriod));

			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker, new DateOnlyAsDateTimePeriod(schedulingDate, timeZoneInfo));

			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2009, 2, 2, 11, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
			var meeting = _mocks.StrictMock<IMeeting>();
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
			var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
			
			Assert.IsFalse(target.PersonalShiftsAndMeetingsAreInWorkTime(new []{personMeeting,personMeeting2}, null));
		}

		[Test]
		public void WorkShiftEndTimeShouldHandleWorkShiftsCrossingMidnight()
		{

			workShift = new WorkShift(category);
			start = new TimeSpan(22, 0, 0);
			end = new TimeSpan(1, 8, 0, 0);
			DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity, period));

			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

			TimeSpan result;

			result = target.WorkShiftEndTime;

			Assert.AreEqual(new TimeSpan(1, 8, 0, 0), result);
		}	
	}
}
