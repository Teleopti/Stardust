using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.DomainTest.Helper;
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

			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker);
			timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time"));
			// blir 7 - 16 med denna tidszon (W. Central Africa Standard Time)
			target.SetDate(schedulingDate, timeZoneInfo);

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
			Assert.AreNotEqual(0, target.DayOfWeek);
		}

		[Test]
		public void VerifySetNewDate()
		{
			Assert.AreEqual(schedulingDate.Date, target.MainShiftProjection.Period().Value.StartDateTime.Date);
			target.SetDate(schedulingDate.AddDays(1), timeZoneInfo);
			Assert.AreEqual(schedulingDate.Date.AddDays(1), target.MainShiftProjection.Period().Value.StartDateTime.Date);
		}

		[Test]
		public void VerifyNoPublicEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
		}

		[Test]
		public void WhenEmptyPersonalShiftsAndMeetingsAreInWorkTimeReturnsTrue()
		{
			var meetings = new List<IPersonMeeting>();
			var readMeetings = new ReadOnlyCollection<IPersonMeeting>(meetings);

			Assert.IsTrue(target.PersonalShiftsAndMeetingsAreInWorkTime(readMeetings, null));
		}

		[Test]
		public void ShouldReturnFalseIfMeetingTimeNotOk()
		{
			var personMeeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{personMeeting});

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
				var result = target.PersonalShiftsAndMeetingsAreInWorkTime(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()), personAssignment);
				Assert.IsFalse(result);
			}	
		}

		[Test]
		public void ShouldReturnTrueIfMeetingAndPersonalShiftTimeOk()
		{
			var personAssignment = _mocks.StrictMock<IPersonAssignment>();
			var personMeeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting });

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

		//[Test]
		//public void CheckIfPersonalShiftsAndMeetingsAreInWorkTimeReturnsTrueWhenAllAreWorkTime()
		//{
		//    var period = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
		//                                    new DateTime(2009, 2, 2, 10, 30, 0, DateTimeKind.Utc));
		//    var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
		//                                     new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

		//    var meetings = new List<IPersonMeeting>();
		//    var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
		//    var meeting = _mocks.StrictMock<IMeeting>();
		//    var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
		//    var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
		//    meetings.Add(personMeeting);
		//    meetings.Add(personMeeting2);


		//    var readMeetings = new ReadOnlyCollection<IPersonMeeting>(meetings);

		//    var persAss = new List<IPersonAssignment>();
		//    var readpersAss = new ReadOnlyCollection<IPersonAssignment>(persAss);

		//    Assert.IsTrue(target.PersonalShiftsAndMeetingsAreInWorkTime(readMeetings, readpersAss));
		//}

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

			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker);
			target.SetDate(schedulingDate, timeZoneInfo);

			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2009, 2, 2, 11, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var meetings = new List<IPersonMeeting>();
			var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
			var meeting = _mocks.StrictMock<IMeeting>();
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
			var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
			meetings.Add(personMeeting);
			meetings.Add(personMeeting2);


			var readMeetings = new ReadOnlyCollection<IPersonMeeting>(meetings);

			Assert.IsFalse(target.PersonalShiftsAndMeetingsAreInWorkTime(readMeetings, null));
		}

		//[Test]
		//public void CheckIfPersonalShiftsAndMeetingsAreInWorkTimeReturnsFalseWhenPersonalShiftOutsideWorkTime()
		//{
		//    IActivity lunch = new Activity("lunch");
		//    lunch.InWorkTime = false;
		//    start = new TimeSpan(12, 0, 0);
		//    end = new TimeSpan(13, 0, 0);
		//    // gets one hour erlier in definied timezone 11 - 12
		//    // and the meeting 11 - 11:30 is not correct 
		//    DateTimePeriod lunchPeriod = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
		//    workShift.LayerCollection.Add(new WorkShiftActivityLayer(lunch, lunchPeriod));

		//    target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker);
		//    target.SetDate(schedulingDate, timeZoneInfo);

		//    var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
		//                                    new DateTime(2009, 2, 2, 8, 30, 0, DateTimeKind.Utc));
		//    var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc),
		//                                     new DateTime(2009, 2, 2, 11, 30, 0, DateTimeKind.Utc));

		//    var meetings = new List<IPersonMeeting>();
		//    //var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
		//    //var meeting = _mocks.StrictMock<IMeeting>();
		//    //var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
		//    //var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
		//    //meetings.Add(personMeeting);
		//    //meetings.Add(personMeeting2);


		//    var readMeetings = new ReadOnlyCollection<IPersonMeeting>(meetings);

		//    var persAss = new List<IPersonAssignment>();
		//    var personAssignment = _mocks.StrictMock<IPersonAssignment>();
		//    persAss.Add(personAssignment);

		//    var personalShift = _mocks.StrictMock<IPersonalShift>();
		//    var layerColl = _mocks.StrictMock<ILayerCollection<IActivity>>();
		//    var personalShift2 = _mocks.StrictMock<IPersonalShift>();
		//    var layerColl2 = _mocks.StrictMock<ILayerCollection<IActivity>>();

		//    var persList = new List<IPersonalShift> {personalShift, personalShift2};
		//    var personalShifts = new ReadOnlyCollection<IPersonalShift>(persList);

		//    var readpersAss = new ReadOnlyCollection<IPersonAssignment>(persAss);
		//    using (_mocks.Record())
		//    {
		//        Expect.Call(personAssignment.PersonalShiftCollection).Return(personalShifts).Repeat.AtLeastOnce();
		//        Expect.Call(personalShift.LayerCollection).Return(layerColl).Repeat.AtLeastOnce();
		//        Expect.Call(layerColl.Period()).Return(period).Repeat.AtLeastOnce();
		//        Expect.Call(personalShift2.LayerCollection).Return(layerColl2).Repeat.AtLeastOnce();
		//        Expect.Call(layerColl2.Period()).Return(period2).Repeat.AtLeastOnce();
		//    }
		//    using (_mocks.Playback())
		//    {
		//        Assert.IsFalse(target.PersonalShiftsAndMeetingsAreInWorkTime(readMeetings, readpersAss));
		//    }
		//}

		[Test]
		public void WorkShiftEndTimeShouldHandleWorkShiftsCrossingMidnight()
		{

			workShift = new WorkShift(category);
			start = new TimeSpan(22, 0, 0);
			end = new TimeSpan(1, 8, 0, 0);
			DateTimePeriod period = new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity, period));

			target = new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker);

			TimeSpan result;

			result = target.WorkShiftEndTime;

			Assert.AreEqual(new TimeSpan(1, 8, 0, 0), result);
		}
	}



}
