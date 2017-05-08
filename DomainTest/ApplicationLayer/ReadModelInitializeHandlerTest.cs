using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

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
		private ICurrentScenario _currentScenario;
		private LegacyFakeEventPublisher _eventPublisher;
		private IDistributedLockAcquirer _distributedLockAquirer;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakePersonRepositoryLegacy2();
			_scheduleProjectionReadOnlyPersister = MockRepository.GenerateMock<IScheduleProjectionReadOnlyPersister>();
			_scheduleDayReadModelRepository = MockRepository.GenerateMock<IScheduleDayReadModelRepository>();
			_personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelPersister>();
			_currentScenario = new FakeCurrentScenario();
			_eventPublisher = new LegacyFakeEventPublisher();
			_distributedLockAquirer = new FakeDistributedLockAcquirer();

			target = new ReadModelInitializeHandler(_personRepository, _scheduleProjectionReadOnlyPersister, _scheduleDayReadModelRepository, _personScheduleDayReadModelRepository, _currentScenario, _eventPublisher, _distributedLockAquirer);
		}

		[Test]
		public void ShouldNotPublishAnyEventsWhenAlreadyInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(true);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				EndDays = 10,
				StartDays = 1
			});

			_eventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishScheduleChangedEventForEachPersonWhenNoneInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			var count = addPeople();

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				EndDays = 10,
				StartDays = 1
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleChangedEvent>().Count().Should().Be.EqualTo(count);
		}

		[Test]
		public void ShouldPublishScheduleInitializeTriggeredEventForScheduleProjectionForEachPersonWhenScheduleProjectionNotInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(false);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);

			var count = addPeople();

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				EndDays = 10,
				StartDays = 1
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

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				EndDays = 10,
				StartDays = 1
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleInitializeTriggeredEventForScheduleDay>().Count().Should().Be.EqualTo(count);
		}

		[Test]
		public void ShouldPublishScheduleInitializeTriggeredEventForPersonScheduleDayForEachPersonWhenPersonScheduleDayNotInitialized()
		{
			_scheduleProjectionReadOnlyPersister.Stub(x => x.IsInitialized()).Return(true);
			_scheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(true);
			_personScheduleDayReadModelRepository.Stub(x => x.IsInitialized()).Return(false);

			var count = addPeople();

			target.Handle(new InitialLoadScheduleProjectionEvent
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				EndDays = 10,
				StartDays = 1
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(count);
			_eventPublisher.PublishedEvents.OfType<ScheduleInitializeTriggeredEventForPersonScheduleDay>().Count().Should().Be.EqualTo(count);
		}

		private int addPeople()
		{
			var number = new Random(DateTime.UtcNow.Millisecond).Next(5, 10);
			for (var i = 0; i < number; i++)
			{
				_personRepository.Add(new Person());
			}
			return number;
		}
	}
}