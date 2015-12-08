using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class PersonAssignmentEventPublishingTest : ISetup
	{
		public WithUnitOfWork UnitOfWork;
		public FakeEventPublisher EventsPublisher;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldPublishActivityAddedEventsWhenAddingActivity()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");

			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				ActivityRepository.Add(activity);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAssignment = new PersonAssignment(person, scenario, "2015-12-07".Date());
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-12-07 8:00".Utc(), "2015-12-07 17:00".Utc()));
				PersonAssignmentRepository.Add(personAssignment);
			});

			EventsPublisher.PublishedEvents.OfType<ActivityAddedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenAddingActivity()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				ActivityRepository.Add(activity);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAssignment = new PersonAssignment(person, scenario, "2015-12-07".Date());
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-12-07 8:00".Utc(), "2015-12-07 17:00".Utc()));
				PersonAssignmentRepository.Add(personAssignment);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}



		[Test]
		public void ShouldPublishActivityMovedEventsWhenMovingActivity()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");

			UnitOfWork.Do(() =>
			{
				var personAssignment = new PersonAssignment(person, scenario, "2015-12-07".Date());
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-12-07 8:00".Utc(), "2015-12-07 10:00".Utc()));
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				ActivityRepository.Add(activity);
				PersonAssignmentRepository.Add(personAssignment);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAssignment = PersonAssignmentRepository.Find(new DateOnlyPeriod("2015-12-07".Date(), "2015-12-08".Date()), scenario).Single();
				personAssignment.MoveActivityAndSetHighestPriority(activity, "2015-12-07 8:00".Utc(), "2015-12-07 9:00".Utc(), "2".Hours(), null);
				PersonAssignmentRepository.Add(personAssignment);
			});

			EventsPublisher.PublishedEvents.OfType<ActivityMovedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenMovingActivity()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");

			UnitOfWork.Do(() =>
			{
				var personAssignment = new PersonAssignment(person, scenario, "2015-12-07".Date());
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-12-07 8:00".Utc(), "2015-12-07 10:00".Utc()));
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				ActivityRepository.Add(activity);
				PersonAssignmentRepository.Add(personAssignment);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAssignment = PersonAssignmentRepository.Find(new DateOnlyPeriod("2015-12-07".Date(), "2015-12-08".Date()), scenario).Single();
				personAssignment.MoveActivityAndSetHighestPriority(activity, "2015-12-07 8:00".Utc(), "2015-12-07 9:00".Utc(), "2".Hours(), null);
				PersonAssignmentRepository.Add(personAssignment);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}
	}
}
