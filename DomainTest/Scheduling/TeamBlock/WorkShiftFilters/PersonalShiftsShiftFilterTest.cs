using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class PersonalShiftsShiftFilterTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _resultStateHolder;
		private IPersonalShiftsShiftFilter _target;
		private IPersonAssignment _personAssignment;
		private IScheduleRange _scheduleRange;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleDay _part;
		private IPerson _person;
		private DateOnly _dateOnly;
		private WorkShiftFinderResult _finderResult;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_part = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson("Bill");
			_resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_finderResult = new WorkShiftFinderResult(_person, _dateOnly);
			_target = new PersonalShiftsShiftFilter(_resultStateHolder);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCoverMeetingAndPersonalShiftsWhenItIsPossible()
		{
			var period = new DateTimePeriod(new DateTime(2013, 3, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 3, 1, 9, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2013, 3, 1, 12, 0, 0, DateTimeKind.Utc),
											 new DateTime(2013, 3, 1, 14, 0, 0, DateTimeKind.Utc));

			var meeting = _mocks.StrictMock<IPersonMeeting>();
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { meeting });
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
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
				Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_part);
				Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
				Expect.Call(_personAssignment.PersonalActivities()).Return(new []{new PersonalShiftLayer(new Activity("sdf"), period)}).Repeat.AtLeastOnce();
				Expect.Call(meeting.Period).Return(period2).Repeat.AtLeastOnce();
				Expect.Call(c1.MainShiftProjection).Return(layerCollection1).Repeat.AtLeastOnce();
				Expect.Call(c1.PersonalShiftsAndMeetingsAreInWorkTime(new ReadOnlyCollection<IPersonMeeting>(meetings),
																	  _personAssignment)).Return(true);
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(_dateOnly, _person, shifts, _finderResult);
				Assert.That(shiftsList.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var shiftsList = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), _finderResult);
			Assert.That(shiftsList.Count, Is.EqualTo(0));
			
			shiftsList = _target.Filter(_dateOnly, _person, null, _finderResult);
			Assert.IsNull(shiftsList);
		}

		[Test]
		public void ShouldSkipIfThereIsNoMeetingAndPersonalShift()
		{
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();

			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;

			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var c1 = _mocks.StrictMock<IShiftProjectionCache>();
			shifts.Add(c1);

			using (_mocks.Record())
			{
				Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_part);
				Expect.Call(_part.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>())).Repeat.AtLeastOnce();
				Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(_dateOnly, _person, shifts, _finderResult);
				Assert.That(shiftsList.Count, Is.EqualTo(1));
			}
		}
	}
}
