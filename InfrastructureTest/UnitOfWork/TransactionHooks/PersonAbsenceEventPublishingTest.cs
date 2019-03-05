using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[DatabaseTest]
	public class PersonAbsenceEventPublishingTest : IIsolateSystem
	{
		public WithUnitOfWork UnitOfWork;
		public FakeEventPublisher EventsPublisher;
		public IScenarioRepository ScenarioRepository;
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldPublishFullDayAbsenceEventWhenAddingFullDayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() {Description = new Description("Jullov")};
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 17:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.FullDayAbsence(person, null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<FullDayAbsenceAddedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenAddingFullDayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 17:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.FullDayAbsence(person, null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldPublishPersonAbscenseAddedEventWhenAddingIntradayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.IntradayAbsence(person, null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<PersonAbsenceAddedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenAddingIntradayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.IntradayAbsence(person, null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldPublishPersonAbscenseAddedEventWhenModifyingAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.ModifyPersonAbsencePeriod(new DateTimePeriod("2015-12-07 09:00".Utc(), "2015-12-07 11:00".Utc()), null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<PersonAbsenceModifiedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenModifyingAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.ModifyPersonAbsencePeriod(new DateTimePeriod("2015-12-07 09:00".Utc(), "2015-12-07 11:00".Utc()), null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}
		

		[Test]
		public void ShouldPublishPersonAbsenceRemovedEventWhenRemovingAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.RemovePersonAbsence(null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<PersonAbsenceRemovedEvent>()
				.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotPublishScheduleChangedEventWhenRemovingAbsence()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();
			var abscense = new Absence() { Description = new Description("Jullov") };
			var abscenseLayer = new AbsenceLayer(abscense, new DateTimePeriod("2015-12-07 08:00".Utc(), "2015-12-07 10:00".Utc()));
			UnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(abscense);
			});
			EventsPublisher.Clear();

			UnitOfWork.Do(() =>
			{
				var personAbscense = new PersonAbsence(person, scenario, abscenseLayer);
				personAbscense.RemovePersonAbsence(null);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbscense);
			});

			EventsPublisher.PublishedEvents.OfType<ScheduleChangedEvent>()
				.Should().Be.Empty();
		}
	}
}