﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class CoverageTests
	{
		[Test]
		public void ScheduleChangedHandlerShouldBeCovered() // dont blame me, there were no tests
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new ISkill[] {});
			person.PersonPeriodCollection.Single().Team = team;
			var scenario = ScenarioFactory.CreateScenarioAggregate(" ", true);
			var period = new DateTimePeriod(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.Today.AddHours(24), DateTimeKind.Utc));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, null, new Dictionary<IPerson, IScheduleRange>());
			var range = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, person, period));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, period);
			range.Add(personAssignment);
			scheduleDictionary.AddTestItem(person, range);
			var bus = MockRepository.GenerateMock<IQuestionablyPublishMoreEvents>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(Arg<IEnumerable<Guid>>.Is.Anything)).Return(new[] {person});
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null)).IgnoreArguments()
			                  .Return(scheduleDictionary);

			var target = new ScheduleChangedHandler(bus, unitOfWorkFactory, scenarioRepository, personRepository, scheduleRepository, new ProjectionChangedEventBuilder());

			target.Handle(new ScheduleChangedEvent {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForPersonScheduleDay {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleDay {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleProjection {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
		}
	}
}
