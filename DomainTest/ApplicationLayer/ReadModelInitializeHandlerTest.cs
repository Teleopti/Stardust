using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class ReadModelInitializeHandlerTest
	{
		private ReadModelInitializeHandler target;
		private FakePersonRepository _personRepository;
		private IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IPersonScheduleDayReadModelPersister _personScheduleDayReadModelRepository;
		private FakeScenarioRepository _currentScenario;
		private LegacyFakeEventPublisher _eventPublisher;
		private IDistributedLockAcquirer _distributedLockAcquirer;
		private FakePersonAssignmentRepository _personAssignmentRepository;
		private FakePersonAbsenceRepository _personAbsenceRepository;
		private IBusinessUnit _businessUnit;
		private ITeam _team;

		[SetUp]
		public void Setup()
		{
			BusinessUnitUsedInTests.Reset();
			_businessUnit = BusinessUnitUsedInTests.BusinessUnit;
			_team = TeamFactory.CreateTeam("teamName", "siteName");
			_team.Site.SetBusinessUnit(_businessUnit);
			_personRepository = new FakePersonRepository(new FakeStorage());
			_scheduleProjectionReadOnlyPersister = MockRepository.GenerateMock<IScheduleProjectionReadOnlyPersister>();
			_scheduleDayReadModelRepository = MockRepository.GenerateMock<IScheduleDayReadModelRepository>();
			_personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelPersister>();
			_currentScenario = new FakeScenarioRepository();
			_currentScenario.Has("Default");
			_eventPublisher = new LegacyFakeEventPublisher();
			_distributedLockAcquirer = new FakeDistributedLockAcquirer();
			_personAssignmentRepository = new FakePersonAssignmentRepository(new FakeStorage());
			_personAbsenceRepository = new FakePersonAbsenceRepository(new FakeStorage());

			target = new ReadModelInitializeHandler(_personRepository, _scheduleProjectionReadOnlyPersister,
				_scheduleDayReadModelRepository, _personScheduleDayReadModelRepository,
				new DefaultScenarioFromRepository(_currentScenario), _eventPublisher, _distributedLockAcquirer,
				_personAssignmentRepository, _personAbsenceRepository);
		}

		[Test]
		public void ShouldNotPublishAnyEventsWhenAlreadyInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(true);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 1
			});

			_eventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishAnyEventsIfNoAgentScheduledInPeriod()
		{
			var now = DateTime.UtcNow;

			addPeople();
			addAbsence(now);
			addAssignment(new DateOnly(now));
			
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 2
			});

			_eventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishEventsWhenAnyScheduledAssignment()
		{
			var now = DateTime.UtcNow;
			var count = addPeople();
			addAssignment(new DateOnly(now));
			
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be(count);
		}

		[Test]
		public void ShouldPublishEventsWhenAnyScheduledAbsence()
		{
			var count = addPeople();
			addAbsence(DateTime.UtcNow);
			
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be(count);
		}

		[Test]
		public void ShouldPublishScheduleChangedEventForEachPersonWhenNoneInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			var count = addPeople();

			var now = DateTime.UtcNow;
			addAssignment(new DateOnly(now));

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleChangedEvent>().Count().Should().Be.EqualTo(count);
		}

		[Test]
		public void ShouldPublishScheduleChangedEventForPersonWithPersonPeriod()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			addPeople(false);

			var now = DateTime.UtcNow;
			addAssignment(new DateOnly(now));
			addPersonPeriod(_personRepository.FindAllSortByName().First());

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishScheduleInitializeTriggeredEventForScheduleProjectionForEachPersonWhenScheduleProjectionNotInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);

			var count = addPeople();

			var now = DateTime.UtcNow;
			addAssignment(new DateOnly(now));

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleInitializeTriggeredEventForScheduleProjection>().Count().Should().Be.EqualTo(count);
		}

		[Test]
		public void ShouldPublishScheduleInitializeTriggeredEventForScheduleDayForEachPersonWhenScheduleDayNotInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(true);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);

			var count = addPeople();

			var now = DateTime.UtcNow;
			addAssignment(new DateOnly(now));

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleInitializeTriggeredEventForScheduleDay>().Count().Should().Be.EqualTo(count);
		}

		[Test]
		public void ShouldPublishEventForEachPersonWhenPersonScheduleDayNotInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(true);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			var count = addPeople();

			var now = DateTime.UtcNow;
			addAssignment(new DateOnly(now));

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = _businessUnit.Id.Value,
				EndDays = 10,
				StartDays = 0
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleInitializeTriggeredEventForPersonScheduleDay>().Count().Should().Be.EqualTo(count);
		}

		private int addPeople(bool withPersonPeriod = true)
		{
			var number = new Random(DateTime.UtcNow.Millisecond).Next(5, 10);
			for (var i = 0; i < number; i++)
			{
				IPerson person = new Person();
				if (withPersonPeriod)
				{
					addPersonPeriod(person);
				}
				_personRepository.Add(person);
			}
			return number;
		}

		private void addAssignment(DateOnly todayUtc)
		{
			_personAssignmentRepository.Has(new PersonAssignment(_personRepository.FindAllSortByName().First(),
				_currentScenario.LoadDefaultScenario(), todayUtc));
		}

		private void addAbsence(DateTime todayUtc)
		{
			var nowWithoutSeconds = new DateTime(todayUtc.Year, todayUtc.Month, todayUtc.Day, todayUtc.Hour, todayUtc.Minute, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(nowWithoutSeconds,
				nowWithoutSeconds.AddDays(1));

			var absence = AbsenceFactory.CreateAbsence("absence");

			_personAbsenceRepository.Has(new PersonAbsence(_personRepository.FindAllSortByName().First(),
				_currentScenario.LoadDefaultScenario(), new AbsenceLayer(absence, period)));
		}

		private void addPersonPeriod(IPerson person)
		{
			var personPeriod = new PersonPeriod(DateOnly.Today, PersonContractFactory.CreatePersonContract(), _team);
			person.AddPersonPeriod(personPeriod);
		}
	}
}