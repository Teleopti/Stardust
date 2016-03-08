using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ProjectionChangedEventPublisherTest
	{
		[Test]
		public void ShouldPublishWithDayOffData()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 02));
			var publisher = new FakePublishEventsFromEventHandlers();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 02), new DayOffTemplate(new Description("Day off", "DO")));
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepositoryLegacy(person), new FakePersonAssignmentReadScheduleStorage(personAssignment), new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
				{
					StartDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value
				});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 02));
			scheduleDay.DayOff.Should().Not.Be.Null();
			scheduleDay.IsFullDayAbsence.Should().Be.False();
			scheduleDay.ShortName.Should().Be("DO");
			scheduleDay.Name.Should().Be("Day off");
		}

		[Test]
		public void ShouldPublishWithDayOffStartTimeAndEndTime()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 08));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var publisher = new FakePublishEventsFromEventHandlers();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 08), new DayOffTemplate(new Description("Day off")));
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepositoryLegacy(person), new FakePersonAssignmentReadScheduleStorage(personAssignment), new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
				{
					StartDateTime = new DateTime(2013, 10, 08, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 10, 08, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value
				});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 08));
			scheduleDay.DayOff.StartDateTime.Should().Be.EqualTo(new DateTime(2013, 10, 07, 22, 0, 0));
			scheduleDay.DayOff.EndDateTime.Should().Be.EqualTo(new DateTime(2013, 10, 08, 22, 0, 0));
		}
		
		[Test]
		public void ShouldPublishWithFullDayAbsence()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 04));
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 10, 04, 2013, 10, 05));
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepositoryLegacy(person), new FakePersonAbsenceReadScheduleStorage(personAbsence), new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
				{
					StartDateTime = new DateTime(2013, 10, 04, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 10, 04, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value
				});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 04));
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldPublishWithFullDayAbsenceForConfidentialAbsence()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 04));
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var absence = AbsenceFactory.CreateAbsence("123");
			absence.Confidential = true;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 10, 04, 2013, 10, 05), absence);
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepositoryLegacy(person), new FakePersonAbsenceReadScheduleStorage(personAbsence), new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = new DateTime(2013, 10, 04, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2013, 10, 04, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
				PersonId = person.Id.Value,
				ScenarioId = scenario.Id.Value
			});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 04));
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldPublishWithFullDayAbsenceOnDayOff()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 29));
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 10, 29, 2013, 10, 30), absence);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 29), new DayOffTemplate(new Description("Day off", "DO")));
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario),
			                                                 new FakePersonRepositoryLegacy(person), new FakeScheduleDataReadScheduleStorage(personAbsence, personAssignment), 
			                                                 new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = new DateTime(2013, 10, 29, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2013, 10, 29, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
				PersonId = person.Id.Value,
				ScenarioId = scenario.Id.Value
			});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 29));
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void Handle_WhenScheduleChangeForACertainPeriod_ShouldCheckTheDayBeforeForProjectionChangesBecauseItCanBeAffectedIfItsANightShift()
		{
			//Bug 23647
			var now = new MutableNow(DateTime.UtcNow);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 29));
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 29), new DayOffTemplate(new Description("Day off", "DO")));
			var scheduleRepository = new FakeScheduleDataReadScheduleStorage(personAssignment);
			
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario),
															 new FakePersonRepositoryLegacy(person), scheduleRepository,
															 new ProjectionChangedEventBuilder(), now);

			var start = new DateTime(2013, 10, 29, 0, 0, 0, DateTimeKind.Utc);
			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = start,
				EndDateTime = start.AddHours(24).AddMinutes(-1),
				PersonId = person.Id.Value,
				ScenarioId = scenario.Id.Value
			});

			scheduleRepository.ThePeriodThatWasUsedForFindingSchedules.StartDateTime.Day.Should().Be.EqualTo(start.Day-1);
		}

		[Test]
		public void ShouldPublishWithScheduleDataLoadTime()
		{
			var now = new MutableNow(DateTime.UtcNow);
			var scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 02));
			var publisher = new FakePublishEventsFromEventHandlers();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 02), new DayOffTemplate(new Description("Day off", "DO")));
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepositoryLegacy(person), new FakePersonAssignmentReadScheduleStorage(personAssignment), new ProjectionChangedEventBuilder(), now);

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
				PersonId = person.Id.Value,
				ScenarioId = scenario.Id.Value
			});

			var projectionChangedEvent = publisher.Published<ProjectionChangedEvent>();
			projectionChangedEvent.ScheduleLoadTimestamp.Should().Be.EqualTo(now.UtcDateTime());
		}
	}
}