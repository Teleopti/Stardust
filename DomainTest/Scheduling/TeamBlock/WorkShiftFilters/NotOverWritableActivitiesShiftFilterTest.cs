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


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	public class NotOverWritableActivitiesShiftFilterTest
	{
		private MockRepository _mocks;
		private readonly DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private INotOverWritableActivitiesShiftFilter _target;
		private IScheduleDay _part;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("Bill");
			_part = ScheduleDayFactory.Create(_dateOnly,_person);
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
			workShift.LayerCollection.Add(getLunchLayer(lunch));

			var shiftProjectionCache = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(_dateOnly,TimeZoneInfo.Utc));
			var shifts = new[] { shiftProjectionCache };
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);

			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts);
			retShifts.Count.Should().Be.EqualTo(0);
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
			_part.Add(meeting.GetPersonMeetings(_part.Period, _person).First());

			var workShift = new WorkShift(new ShiftCategory("Day"));
			workShift.LayerCollection.Add(getLunchLayer(lunch));

			var shiftProjectionCache = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Utc));
			var shifts = new []{shiftProjectionCache};

			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(schedules[_person].ScheduledDay(_dateOnly)).Return(_part);
			_mocks.ReplayAll();
			var retShifts = _target.Filter(schedules, _dateOnly, _person, shifts);
			retShifts.Count.Should().Be.EqualTo(0);
			_mocks.VerifyAll();
		}
		
		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(null, _dateOnly, _person, null);
			Assert.IsNull(result);

			result = _target.Filter(null, _dateOnly, null, new List<ShiftProjectionCache>());
			Assert.IsNull(result);

			result = _target.Filter(null, _dateOnly, _person, new List<ShiftProjectionCache>());
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private static WorkShiftActivityLayer getLunchLayer(IActivity lunch)
		{
			return new WorkShiftActivityLayer(lunch, new DateTimePeriod(WorkShift.BaseDate.AddHours(11), WorkShift.BaseDate.AddHours(12)));
		}
	}
}
