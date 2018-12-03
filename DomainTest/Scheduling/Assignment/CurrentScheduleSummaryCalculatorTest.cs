using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class CurrentScheduleSummaryCalculatorTest
	{
		public IUserTimeZone UserTimeZone;
		
		[Test]
		public void ShouldCalculateCorrectWhenLoggedOnInSwedenAndAgentInFinland()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"));

			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.InContractTime = true;
			var shiftCategory = new ShiftCategory("_").WithId();
			var loggedOnTimeZone = UserTimeZone.TimeZone();
			var date = new DateOnly(2016, 01, 28);
			var loggedOnDateTime = TimeZoneHelper.ConvertToUtc(date.Date, loggedOnTimeZone);
			var visibleDateTimePeriod = new DateTimePeriod(loggedOnDateTime, loggedOnDateTime.AddDays(1));
			var visibleDateOnlyPeriod = visibleDateTimePeriod.ToDateOnlyPeriod(loggedOnTimeZone);
			var scenario = new Scenario("scenario");
			var dic = new ScheduleDictionaryForTest(scenario, visibleDateTimePeriod);
			var scheduleParameters = new ScheduleParameters(scenario, person, new DateTimePeriod(2016, 01, 26, 2016, 01, 30));
			var range = new ScheduleRange(dic, scheduleParameters, new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());

			var personDateTime = TimeZoneHelper.ConvertToUtc(date.AddDays(-1).Date, person.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(person, scenario, date.AddDays(-1));
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(11)));
			range.Add(assignment);

			personDateTime = TimeZoneHelper.ConvertToUtc(date.Date, person.PermissionInformation.DefaultTimeZone());
			assignment = new PersonAssignment(person, scenario, date);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(17)));
			range.Add(assignment);

			personDateTime = TimeZoneHelper.ConvertToUtc(date.AddDays(1).Date, person.PermissionInformation.DefaultTimeZone());
			assignment = new PersonAssignment(person, scenario, date.AddDays(1));
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(12)));
			range.Add(assignment);

			var currentContractTimeOnVisiblePeriod = range.CalculatedContractTimeHolderOnPeriod(visibleDateOnlyPeriod);
			currentContractTimeOnVisiblePeriod.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldCalculateCorrectWhenLoggedOnInSwedenAndAgentInLondon()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.InContractTime = true;
			var shiftCategory = new ShiftCategory("_").WithId();
			var loggedOnTimeZone = UserTimeZone.TimeZone();
			var date = new DateOnly(2016, 01, 28);
			var loggedOnDateTime = TimeZoneHelper.ConvertToUtc(date.Date, loggedOnTimeZone);
			var visibleDateTimePeriod = new DateTimePeriod(loggedOnDateTime, loggedOnDateTime.AddDays(1));
			var visibleDateOnlyPeriod = visibleDateTimePeriod.ToDateOnlyPeriod(loggedOnTimeZone);
			var scenario = new Scenario("scenario");
			
			var dic = new ScheduleDictionaryForTest(scenario, visibleDateTimePeriod);
			var scheduleParameters = new ScheduleParameters(scenario, person, new DateTimePeriod(2016, 01, 26, 2016, 01, 30));
			var range = new ScheduleRange(dic, scheduleParameters, new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());

			var personDateTime = TimeZoneHelper.ConvertToUtc(date.AddDays(-1).Date, person.PermissionInformation.DefaultTimeZone());
			var assignment = new PersonAssignment(person, scenario, date.AddDays(-1));
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(11)));
			range.Add(assignment);

			personDateTime = TimeZoneHelper.ConvertToUtc(date.Date, person.PermissionInformation.DefaultTimeZone());
			assignment = new PersonAssignment(person, scenario, date);
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(17)));
			range.Add(assignment);

			personDateTime = TimeZoneHelper.ConvertToUtc(date.AddDays(1).Date, person.PermissionInformation.DefaultTimeZone());
			assignment = new PersonAssignment(person, scenario, date.AddDays(1));
			assignment.SetShiftCategory(shiftCategory);
			assignment.AddActivity(phoneActivity, new DateTimePeriod(personDateTime.AddHours(9), personDateTime.AddHours(12)));
			range.Add(assignment);

			var currentContractTimeOnVisiblePeriod = range.CalculatedContractTimeHolderOnPeriod(visibleDateOnlyPeriod);
			currentContractTimeOnVisiblePeriod.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}
	}
}