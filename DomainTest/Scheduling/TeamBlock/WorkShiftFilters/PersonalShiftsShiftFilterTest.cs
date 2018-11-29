using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class PersonalShiftsShiftFilterTest
	{
		private MockRepository _mocks;
		private PersonalShiftsShiftFilter _target;
		private IScheduleDay _part;
		private IPerson _person;
		private readonly DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("Bill");
			_part = ScheduleDayFactory.Create(_dateOnly,_person);
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new PersonalShiftsShiftFilter(_personalShiftMeetingTimeChecker);
		}

		[Test]
		public void ShouldCoverMeetingAndPersonalShiftsWhenItIsPossible()
		{
			var currentDate = DateTime.SpecifyKind(_dateOnly.Date,DateTimeKind.Utc);
			var period = new DateTimePeriod(new DateTime(2013, 3, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 3, 1, 9, 30, 0, DateTimeKind.Utc));
			
			var meeting = new Meeting(PersonFactory.CreatePerson(), new[] { new MeetingPerson(_person, false) }, "subject",
				"location", "", ActivityFactory.CreateActivity("Training"), _part.Scenario)
			{
				StartDate = _dateOnly,
				EndDate = _dateOnly,
				StartTime = TimeSpan.FromHours(12),
				EndTime = TimeSpan.FromHours(14)
			};

			var personMeetings = meeting.GetPersonMeetings(_part.Period, _person);
			_part.CreateAndAddPersonalActivity(new Activity("sdf"), period);
			_part.Add(personMeetings.First());
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			schedules.Expect(x => x[_person].ScheduledDay(_dateOnly)).Return(_part);

			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;
			
			var workShift = new WorkShift(new ShiftCategory("hepp"));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17))));
			var c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Utc));
			var shifts = new List<ShiftProjectionCache> {c1};

			using (_mocks.Record())
			{
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimeMeeting(null, _part.PersonMeetingCollection(true))).IgnoreArguments().Return(true);
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(null, _part.PersonAssignment())).IgnoreArguments()
					.Return(true);
			}

			using (_mocks.Playback())
			{
				var shiftsList = _target.Filter(schedules, _dateOnly, _person, shifts);
				Assert.That(shiftsList.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldHandleMeetingWithNoPersonAssignment()
		{
			var currentDate = DateTime.SpecifyKind(_dateOnly.Date, DateTimeKind.Utc);
			
			var meeting = new Meeting(PersonFactory.CreatePerson(), new[] { new MeetingPerson(_person, false) }, "subject",
				"location", "", ActivityFactory.CreateActivity("Training"), _part.Scenario)
			{
				StartDate = _dateOnly,
				EndDate = _dateOnly,
				StartTime = TimeSpan.FromHours(12),
				EndTime = TimeSpan.FromHours(14)
			};

			var personMeetings = meeting.GetPersonMeetings(_part.Period, _person);
			_part.Add(personMeetings.First());
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			schedules.Expect(x => x[_person].ScheduledDay(_dateOnly)).Return(_part);

			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;

			var workShift = new WorkShift(new ShiftCategory("hepp"));
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(phone, new DateTimePeriod(currentDate.AddHours(8), currentDate.AddHours(17))));
			var c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Utc));
			var shifts = new List<ShiftProjectionCache> { c1 };
			
			using (_mocks.Record())
			{
				Expect.Call(_personalShiftMeetingTimeChecker.CheckTimeMeeting(null, _part.PersonMeetingCollection(true))).IgnoreArguments().Return(true);
			}
			using (_mocks.Record())
			{
				var shiftList = _target.Filter(schedules, _dateOnly, _person, shifts);
				Assert.That(shiftList.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			schedules.Expect(x => x[_person].ScheduledDay(_dateOnly)).Return(_part);
			var shiftsList = _target.Filter(schedules, _dateOnly, _person, new List<ShiftProjectionCache>());
			Assert.That(shiftsList.Count, Is.EqualTo(0));
			
			shiftsList = _target.Filter(schedules, _dateOnly, _person, null);
			Assert.IsNull(shiftsList);
		}

		[Test]
		public void ShouldSkipIfThereIsNoMeetingAndPersonalShift()
		{
			var phone = ActivityFactory.CreateActivity("phone");
			phone.AllowOverwrite = true;
			phone.InWorkTime = true;

			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
			var c1 = new ShiftProjectionCache(new WorkShift(ShiftCategoryFactory.CreateShiftCategory()), new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			shifts.Add(c1);
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			schedules.Expect(x => x[_person].ScheduledDay(_dateOnly)).Return(_part);

			var shiftsList = _target.Filter(schedules, _dateOnly, _person, shifts);
			Assert.That(shiftsList.Count, Is.EqualTo(1));
		}
	}
}
