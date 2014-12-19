using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class NonMainShiftActivityRuleTest
	{
		 [Test]
		 public void ShouldResultInNoBusinessRuleRespone()
		 {
			 var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			 var pa = new PersonAssignment(scheduleOk.Person, scheduleOk.Scenario, new DateOnly(2000, 1, 1));
			 var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			 var end = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			 pa.AddActivity(new Activity("d"), new DateTimePeriod(start, end));
			 scheduleOk.Add(pa);

			 new NonMainShiftActivityRule()
					.Validate(null, new[] {scheduleOk})
					.Should().Be.Empty();
		 }

		[Test]
		public void ShouldReturnCorrectResponseWhenHasPersonalActivity()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithPersonalActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithPersonalActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
					                         scheduleDataWithPersonalActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			var targetRel = new NonMainShiftActivityRule()
				.Validate(null, new[] {scheduleDataWithPersonalActivity});

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldReturnCorrectResponseWhenHasOvertimeActivity()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithOvertimeActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithOvertimeActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithOvertimeActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithOvertimeActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithOvertimeActivity.Person, scheduleDataWithOvertimeActivity.Scenario, dateOnly);
			pa.AddOvertimeActivity(new Activity("p"), assignmentPeriod, new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime));
			scheduleDataWithOvertimeActivity.Add(pa);

			var targetRel = new NonMainShiftActivityRule()
				.Validate(null, new[] { scheduleDataWithOvertimeActivity });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldReturnCorrectResponseWhenHasMeeting()
		{
			var personMeeting = MockRepository.GenerateMock<IPersonMeeting>();
			var personMeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{personMeeting});
			var scheduleDataWithMeeting = MockRepository.GenerateMock<IScheduleDay>();
			var pa = MockRepository.GenerateMock<IPersonAssignment>();
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson();

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			pa.Stub(p => p.Date).Return(dateOnly);
			scheduleDataWithMeeting.Stub(s => s.Person).Return(person);
			scheduleDataWithMeeting.Stub(s => s.PersonAssignment()).Return(pa);
			scheduleDataWithMeeting.Stub(s => s.PersonMeetingCollection()).Return(personMeetings);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithMeeting.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithMeeting.Person, period);

			var targetRel = new NonMainShiftActivityRule()
				.Validate(null, new[] { scheduleDataWithMeeting });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldBeOkWithNoAssignment()
		{
			var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			new DataPartOfAgentDay()
				 .Validate(null, new[] { scheduleOk })
				 .Should().Be.Empty();
		}
		
		[Test]
		public void ShouldResultInTwoResponses()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithPersonalActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithPersonalActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
					                         scheduleDataWithPersonalActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			var targetRel = new NonMainShiftActivityRule()
				.Validate(null, new[] {scheduleDataWithPersonalActivity, scheduleDataWithPersonalActivity});

			targetRel.Should().Have.SameValuesAs(expected, expected);
		}
		
	}
}