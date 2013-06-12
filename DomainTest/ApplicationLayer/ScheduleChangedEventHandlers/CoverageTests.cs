using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	// dont blame me, there were no tests
	[TestFixture]
	public class CoverageTests
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ScheduleChangedHandlerShouldBeCovered()
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
			var bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(Arg<IEnumerable<Guid>>.Is.Anything)).Return(new[] {person});
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null)).IgnoreArguments()
			                  .Return(scheduleDictionary);

			var target = new ScheduleChangedHandler(bus, scenarioRepository, personRepository, scheduleRepository, new ProjectionChangedEventBuilder());

			target.Handle(new FullDayAbsenceAddedEvent { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleChangedEvent {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForPersonScheduleDay {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleDay {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleProjection {StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ScheduleChangedHandlerShouldBeCovered4()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new ISkill[] { });
			person.PersonPeriodCollection.Single().Team = team;
			var scenario = ScenarioFactory.CreateScenarioAggregate(" ", true);
			var period = new DateTimePeriod(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.Today.AddHours(24), DateTimeKind.Utc));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, null, new Dictionary<IPerson, IScheduleRange>());
			var range = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, person, period));
			var personDayOff = new PersonDayOff(person, scenario, DayOffFactory.CreateDayOff(), new DateOnly(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc)), TimeZoneInfoFactory.UtcTimeZoneInfo());
			range.Add(personDayOff);
			scheduleDictionary.AddTestItem(person, range);
			var bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(Arg<IEnumerable<Guid>>.Is.Anything)).Return(new[] { person });
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null)).IgnoreArguments()
							  .Return(scheduleDictionary);

			var target = new ScheduleChangedHandler(bus, scenarioRepository, personRepository, scheduleRepository, new ProjectionChangedEventBuilder());

			target.Handle(new ScheduleChangedEvent { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForPersonScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleProjection { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ScheduleChangedHandlerShouldBeCovered2()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate(" ", false);
			var bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);

			var target = new ScheduleChangedHandler(bus, scenarioRepository, null, null, null);

			target.Handle(new ScheduleChangedEvent { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForPersonScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleProjection { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ScheduleChangedHandlerShouldBeCovered3()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate(" ", true);
			var bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(new Guid[] {})).IgnoreArguments().Return(new Collection<IPerson>());

			var target = new ScheduleChangedHandler(bus, scenarioRepository, personRepository, null, null);

			target.Handle(new ScheduleChangedEvent { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForPersonScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleDay { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
			target.Handle(new ScheduleInitializeTriggeredEventForScheduleProjection { StartDateTime = DateTime.UtcNow, EndDateTime = DateTime.UtcNow });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void PersonScheduleDayReadModelHandlerShouldBeCovered()
		{
			var person = PersonFactory.CreatePerson(" ");
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Load(Arg<Guid>.Is.Anything)).Return(person);
			var jsonSerializer = MockRepository.GenerateMock<IJsonSerializer>();
			var repository = MockRepository.GenerateMock<IPersonScheduleDayReadModelPersister>();

			var target = new PersonScheduleDayReadModelHandler(new PersonScheduleDayReadModelsCreator(personRepository, jsonSerializer), repository);

			var layers = new List<ProjectionChangedEventLayer> {new ProjectionChangedEventLayer()};
			var scheduleDays = new List<ProjectionChangedEventScheduleDay> { new ProjectionChangedEventScheduleDay { Layers = layers } };
			target.Handle(new ProjectionChangedEvent { IsDefaultScenario = true, ScheduleDays = scheduleDays });
			target.Handle(new ProjectionChangedEventForPersonScheduleDay { IsDefaultScenario = true, ScheduleDays = scheduleDays });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "bus"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void UpdateScheduleProjectionReadModelShouldHaveCoverage()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new ISkill[] { });
			person.PersonPeriodCollection.Single().Team = team;
			var scenario = ScenarioFactory.CreateScenarioAggregate(" ", true);
			var period = new DateTimePeriod(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.Today.AddHours(24), DateTimeKind.Utc));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, null, new Dictionary<IPerson, IScheduleRange>());
			var range = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, person, period));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, period);
			range.Add(personAssignment);
			scheduleDictionary.AddTestItem(person, range);
			var bus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(Arg<Guid>.Is.Anything)).Return(scenario);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeople(Arg<IEnumerable<Guid>>.Is.Anything)).Return(new[] { person });
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null)).IgnoreArguments()
							  .Return(scheduleDictionary);
			var repository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();

			var target = new UpdateScheduleProjectionReadModel(new ProjectionChangedEventBuilder(), repository);

			target.Execute(range, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
		}
	}
}
