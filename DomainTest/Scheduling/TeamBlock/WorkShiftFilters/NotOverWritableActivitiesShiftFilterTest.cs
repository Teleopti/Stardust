using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class NotOverWritableActivitiesShiftFilterTest
	{
		private MockRepository _mocks;
		private readonly DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private INotOverWritableActivitiesShiftFilter _target;
		private IScheduleDay _part;
		private WorkShiftFinderResult _finderResult;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("Bill");
			_part = ScheduleDayFactory.Create(_dateOnly,_person);
			_finderResult = new WorkShiftFinderResult(_person, new DateOnly(2009, 2, 3));
			_target = new NotOverWritableActivitiesShiftFilter();
		}

		[Test]
		public void CanFilterOutShiftsWhichCannotBeOverwritten()
		{
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			lunch.InContractTime = true;
			lunch.InWorkTime = true;

			_part.CreateAndAddActivity(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)), ShiftCategoryFactory.CreateShiftCategory());
			_part.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity("personal"),
																		new DateTimePeriod(currentDate.AddHours(10),
																						   currentDate.AddHours(13)));

			var workShift = new WorkShift(new ShiftCategory("Day"));
			workShift.LayerCollection.Add(getLunchLayer(currentDate, lunch));

			var shiftProjectionCache = new ShiftProjectionCache(workShift, new PersonalShiftMeetingTimeChecker());
			shiftProjectionCache.SetDate(new DateOnlyAsDateTimePeriod(_dateOnly,TimeZoneInfo.Utc));
			var shifts = new[] { shiftProjectionCache };
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);

			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoMeeting()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
			var c1 = _mocks.StrictMock<ShiftProjectionCache>();
			shifts.Add(c1);
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);


			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyIfMeetingCannotOverrideActivity()
		{
			var currentDate = new DateTime(2013, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.AllowOverwrite = false;
			lunch.InContractTime = true;
			lunch.InWorkTime = true;

			var meeting = new Meeting(PersonFactory.CreatePerson(), new[] {new MeetingPerson(_person, false)}, "subject",
				"location", "", ActivityFactory.CreateActivity("Training"), _part.Scenario)
			{
				StartDate = _dateOnly,
				EndDate = _dateOnly,
				StartTime = TimeSpan.FromHours(11),
				EndTime = TimeSpan.FromHours(12)
			};

			_part.CreateAndAddActivity(lunch, new DateTimePeriod(currentDate.AddHours(11), currentDate.AddHours(12)),
				ShiftCategoryFactory.CreateShiftCategory());
			_part.Add(meeting.GetPersonMeetings(_person).First());

			var workShift = new WorkShift(new ShiftCategory("Day"));
			workShift.LayerCollection.Add(getLunchLayer(currentDate,lunch));

			var shiftProjectionCache = new ShiftProjectionCache(workShift,new PersonalShiftMeetingTimeChecker());
			shiftProjectionCache.SetDate(new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Utc));
			var shifts = new []{shiftProjectionCache};

			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);
			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotFilterIfNoPersonalShift()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
			var c1 = _mocks.StrictMock<ShiftProjectionCache>();
			shifts.Add(c1);
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);
			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts, _finderResult);
			retShifts.Count.Should().Be.EqualTo(1);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(null, _dateOnly, _person, null, _finderResult);
			Assert.IsNull(result);

			result = _target.Filter(null, _dateOnly, null, new List<ShiftProjectionCache>(), _finderResult);
			Assert.IsNull(result);

			result = _target.Filter(null, _dateOnly, _person, new List<ShiftProjectionCache>(), null);
			Assert.IsNull(result);

			result = _target.Filter(null, _dateOnly, _person, new List<ShiftProjectionCache>(), _finderResult);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private static WorkShiftActivityLayer getLunchLayer(DateTime currentDate, IActivity lunch)
		{
			return new WorkShiftActivityLayer(lunch, new DateTimePeriod(WorkShift.BaseDate.AddHours(11), WorkShift.BaseDate.AddHours(12)));
		}
	}
}
