﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
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
			var scenario = ScenarioFactory.CreateScenarioWithId(" ", true);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 02));
			var publisher = new FakePublishEventsFromEventHandlers();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 02), new DayOffTemplate(new Description("Day off", "DO")));
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepository(person), new FakePersonAssignmentReadScheduleRepository(personAssignment), new ProjectionChangedEventBuilder());

			target.Handle(new ScheduleChangedEvent
				{
					StartDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value
				});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 02));
			scheduleDay.DayOff.Should().Not.Be.Null();
			scheduleDay.ShortName.Should().Be("DO");
			scheduleDay.Name.Should().Be("Day off");
		}

		[Test]
		public void ShouldPublishWithFullDayAbsence()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 04));
			var scenario = ScenarioFactory.CreateScenarioWithId(" ", true);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 10, 04, 2013, 10, 05));
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ProjectionChangedEventPublisher(publisher, new FakeScenarioRepository(scenario), new FakePersonRepository(person), new FakePersonAbsenceReadScheduleRepository(personAbsence), new ProjectionChangedEventBuilder());

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
	}
}