using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;

using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	[DomainTest]
	public class SiteOpenHoursRuleTest
	{
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private readonly Scenario _scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
		private readonly SiteOpenHoursRule _target = new SiteOpenHoursRule(new SiteOpenHoursSpecification());

		[Test]
		public void ShouldValidateWithOpenHourIsSatisfied()
		{
			var person = createPersonWithSiteOpenHours(8, 17);
			var result = executeValidate(person, 8, 17);
			result.Count().Should().Be(0);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldReturnResponseWhenStartHourIsNotSatisfied()
		{
			var person = createPersonWithSiteOpenHours(8, 17);
			var result = executeValidate(person, 7, 17).ToArray();
			result.Length.Should().Be(1);

			var response = result.First();
			response.Person.Equals(person);
			Assert.IsTrue(response.FriendlyName.StartsWith("Aktivitet utanför"));
			Assert.IsTrue(response.Message.StartsWith("Inga öppettider för"));
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
				{person, createScheduleRange(person, schedulDate, new TimePeriod(8, 0, 17, 0))}
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

		[Test]
		public void ShouldValidateWithNightShift()
		{
			var person = createPersonWithSiteOpenHours(new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(33))}
			});
			var result = executeValidate(person,
				new TimePeriod(TimeSpan.FromHours(22), TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5))));
			result.Count().Should().Be(0);
		}

		private IEnumerable<IBusinessRuleResponse> executeValidate(IPerson person, int startHour, int endHour)
		{
			return executeValidate(person, new TimePeriod(startHour, 0, endHour, 0));
		}

		private IEnumerable<IBusinessRuleResponse> executeValidate(IPerson person, TimePeriod timePeriod)
		{
			var schedulDate = new DateOnly(2016, 8, 8);

			var person1ScheduleRange = createScheduleRange(person, schedulDate, timePeriod);

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

		private IScheduleRange createScheduleRange(IPerson person, DateOnly schedulDate, TimePeriod timePeriod)
		{
			var period = schedulDate.ToDateTimePeriod(timePeriod, person.PermissionInformation.DefaultTimeZone());
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, _scenario, schedulDate);
			personAssignment.AddActivity(new Activity("activity"), period);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(_scenario, period, personAssignment);
			var personSchedule = new ScheduleRange(scheduleDictionary,
				new ScheduleParameters(personAssignment.Scenario, person, period), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
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

		private IPerson createPersonWithSiteOpenHours(Dictionary<DayOfWeek, TimePeriod> openHours)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			foreach (var openHour in openHours)
			{
				var siteOpenHour = new SiteOpenHour()
				{
					Parent = team.Site,
					TimePeriod = openHour.Value,
					WeekDay = openHour.Key
				};
				team.Site.AddOpenHour(siteOpenHour);
			}
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}
	}
}
