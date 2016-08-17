using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture, DomainTest]
	public class SiteOpenHoursRuleTest
	{
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly Scenario _scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
		private readonly SiteOpenHoursRule _target = new SiteOpenHoursRule();

		[Test]
		public void ShouldValidateWithOpenHourIsSatisfied()
		{
			var person = createPersonWithSiteOpenHours(8, 17);
			var result = executeValidate(person, 8, 17);
			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnResponseWhenStartHourIsNotSatisfied()
		{
			var person = createPersonWithSiteOpenHours(8, 17);
			var result = executeValidate(person, 7, 17);
			result.Count().Should().Be(1);
			var response = result.FirstOrDefault();
			response.Person.Equals(person);
		}

		[Test]
		public void ShouldReturnResponseWhenEndHourIsNotSatisfied()
		{
			var person = createPersonWithSiteOpenHours(8, 17);
			var result = executeValidate(person, 8, 18);
			result.Count().Should().Be(1);
			var response = result.FirstOrDefault();
			response.Person.Equals(person);
		}

		[Test]
		public void ShouldValidateWithEmptyScheduleDay()
		{
			var schedulDate = new DateOnly(2016, 8, 8);
			var person = createPersonWithSiteOpenHours(8, 17);
			var personScheduleRangeDictionary = new Dictionary<IPerson, IScheduleRange>
			{
				{person, createScheduleRange(person, schedulDate, 8, 17)}
			};
			var scheduleDays = new List<IScheduleDay>
			{
				ScheduleDayFactory.Create(schedulDate, person, _scenario)
			};
			var result = _target.Validate(personScheduleRangeDictionary, scheduleDays);
			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnResponseWhenSiteOpenHoursIsClosed()
		{
			var person = createPersonWithSiteOpenHours(8, 17, true);
			var result = executeValidate(person, 7, 16);
			result.Count().Should().Be(1);
			var response = result.FirstOrDefault();
			response.Person.Equals(person);
		}

		private IEnumerable<IBusinessRuleResponse> executeValidate(IPerson person, int startHour, int endHour)
		{
			var schedulDate = new DateOnly(2016, 8, 8);

			var person1ScheduleRange = createScheduleRange(person, schedulDate, startHour, endHour);

			var personScheduleRangeDictionary = new Dictionary<IPerson, IScheduleRange>
			{
				{person, person1ScheduleRange}
			};
			var scheduleDays = new List<IScheduleDay>
			{
				person1ScheduleRange.ScheduledDay(schedulDate)
			};
			return _target.Validate(personScheduleRangeDictionary, scheduleDays);
		}

		private IScheduleRange createScheduleRange(IPerson person, DateOnly schedulDate, int startHour, int endHour)
		{
			var timePeriod = new TimePeriod(startHour, 0, endHour, 0);
			var period = schedulDate.ToDateTimePeriod(timePeriod, person.PermissionInformation.DefaultTimeZone());
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, _scenario, schedulDate);
			personAssignment.AddActivity(new Activity("activity"), period);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(_scenario, period, personAssignment);
			var personSchedule = new ScheduleRange(scheduleDictionary,
				new ScheduleParameters(personAssignment.Scenario, person, period), new PersistableScheduleDataPermissionChecker());
			personSchedule.Add(personAssignment);
			return personSchedule;
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour()
			{
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday,
				IsClosed = isOpenHoursClosed
			};
			team.Site.AddOpenHour(siteOpenHour);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}
	}
}
