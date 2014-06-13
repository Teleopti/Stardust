using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class PersonalShiftAndMeetingFilterTest
	{
		private MockRepository _mocks;
		private PersonalShiftAndMeetingFilter _target;
		private IScheduleDay _part;
		private IPersonAssignment _personAssignment;
		private IWorkShiftFinderResult _finderResult;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new PersonalShiftAndMeetingFilter();
			_part = _mocks.StrictMock<IScheduleDay>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_finderResult = new WorkShiftFinderResult(new Person(), new DateOnly(2009, 2, 3));
		}

		[Test]
		public void CanGetMaximumPeriodForMeetings()
		{
			var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
								new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			ReadOnlyCollection<IPersonMeeting> meetings = getMeetings();

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(Enumerable.Empty<IPersonalShiftLayer>()).Repeat.AtLeastOnce();
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
			var resultPeriod = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
								new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));

			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));


			var retList = new List<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(retList);

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
			var retList = new List<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(retList);

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
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc));

			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			var currentDate = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;
			var phoneLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17)),
                                                     phone, null)
                                 };
			var layerCollection1 = new VisualLayerCollection(null, phoneLayer, new ProjectionPayloadMerger());

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period)
	                }).Repeat.AtLeastOnce();
				Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
				Expect.Call(c1.PersonalShiftsAndMeetingsAreInWorkTime(new ReadOnlyCollection<IPersonMeeting>(meetings),
																	  _personAssignment)).Return(true);
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(shifts, _part, _finderResult);
				Assert.That(shiftsList.Count, Is.EqualTo(1));
			}
		}


		[Test]
		public void ShouldGetNoShiftWhenNoMainShiftCanCoverMeetingAndPersonalShifts()
		{
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 17, 15, 0, DateTimeKind.Utc));

			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			var currentDate = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;
			var phoneLayer = new List<IVisualLayer>
                                 {
                                     new VisualLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17)),
                                                     phone, null)
                                 };
			var layerCollection1 = new VisualLayerCollection(null, phoneLayer, new ProjectionPayloadMerger());

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);

			using (_mocks.Record())
			{
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new[]
	                {
		                new PersonalShiftLayer(new Activity("d"), period)
	                }).Repeat.AtLeastOnce();
				Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(shifts, _part, _finderResult);
				Assert.That(shiftsList.Count, Is.EqualTo(0));
			}
		}

		private ReadOnlyCollection<IPersonMeeting> getMeetings()
		{
			var period = new DateTimePeriod(new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc),
											new DateTime(2009, 2, 2, 10, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2009, 2, 2, 14, 0, 0, DateTimeKind.Utc),
											 new DateTime(2009, 2, 2, 14, 30, 0, DateTimeKind.Utc));

			var retList = new List<IPersonMeeting>();
			var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
			var meeting = _mocks.StrictMock<IMeeting>();
			var personMeeting = new PersonMeeting(meeting, meetingPerson, period);
			var personMeeting2 = new PersonMeeting(meeting, meetingPerson, period2);
			retList.Add(personMeeting);
			retList.Add(personMeeting2);
			return new ReadOnlyCollection<IPersonMeeting>(retList);
		}

	}
}