using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class PersonalShiftAndMeetingFilterTest
	{
		private MockRepository _mocks;
		private PersonalShiftAndMeetingFilter _target;
		private IScheduleDay _part;
		private IPersonAssignment _personAssignment;

		public void setup()
		{
			_mocks = new MockRepository();
			_target = new PersonalShiftAndMeetingFilter(new PersonalShiftMeetingTimeChecker());
			_part = _mocks.StrictMock<IScheduleDay>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
		}

		[Test]
		public void CanGetMaximumPeriodForMeetings()
		{
			setup();
			var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
								new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var meetings = getMeetings();

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(Enumerable.Empty<PersonalShiftLayer>()).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
				Assert.AreEqual(resultPeriod, retPeriod);
			}
		}

		[Test]
		public void CanGetMaximumPeriodForPersonalShifts()
		{
			setup();
			var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
								new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));

			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));
			
			var meetings = new IPersonMeeting[0];

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities())
					  .Return(new[]
		                  {
			                  new PersonalShiftLayer(new Activity("sdf"), period), 
			                  new PersonalShiftLayer(new Activity("sdf"), period2)
		                  })
					  .Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
				Assert.AreEqual(resultPeriod, retPeriod);
			}

		}

		[Test]
		public void CanGetMaximumPeriodForPersonalShiftsAndMeetings()
		{
			setup();
			var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
								new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));


			var meetings = getMeetings();

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period), 
		                new PersonalShiftLayer(new Activity("d"), period2)
	                }).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
				Assert.AreEqual(resultPeriod, retPeriod);
			}

		}

		[Test]
		public void GetMaximumPeriodForPersonalShiftsAndMeetingsReturnsNullWhenEmpty()
		{
			setup();
			var meetings = new IPersonMeeting[0];

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(null);
			}

			using (_mocks.Playback())
			{
				var retPeriod = _target.GetMaximumPeriodForPersonalShiftsAndMeetings(_part);
				Assert.IsNull(retPeriod);
			}
		}

		[Test]
		public void ShouldCoverMeetingAndPersonalShiftsWhenItIsPossible()
		{
			setup();
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));

			var origMeeting = _mocks.DynamicMock<IMeeting>();
			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new [] { meeting };
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			
			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;

			var workShift = new WorkShift(new ShiftCategory("Day"));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(phone,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(17))));

			var shiftProjectionCache = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(new DateOnly(2009,2,2), TimeZoneInfo.Utc));
			var shifts = new []{shiftProjectionCache };
			
			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"){InWorkTime = true}, period)
	                }).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.Period).Return(period.MaximumPeriod(period2)).Repeat.AtLeastOnce();
				Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(meeting.BelongsToMeeting).Return(origMeeting).Repeat.AtLeastOnce();
				Expect.Call(origMeeting.Activity).Return(phone).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(shifts, _part);
				Assert.That(shiftsList.Count, Is.EqualTo(1));
			}
		}


		[Test]
		public void ShouldGetNoShiftWhenNoMainShiftCanCoverMeetingAndPersonalShifts()
		{
			setup();
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 17, 15, 0, DateTimeKind.Utc));

			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new []{ meeting };
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			
			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;
			
			var workShift = new WorkShift(new ShiftCategory("Day"));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(phone,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(17))));

			var shiftProjectionCache = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), TimeZoneInfo.Utc));
			var shifts = new[] { shiftProjectionCache };

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period)
	                }).Repeat.AtLeastOnce();
				Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(shifts, _part);
				Assert.That(shiftsList.Count, Is.EqualTo(0));
			}
		}

		private IPersonMeeting[] getMeetings()
		{
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 10, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
			var meeting = _mocks.StrictMock<IMeeting>();
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
			var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
			return new []{personMeeting,personMeeting2};
		}

	}
}