using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionConsumerTest
	{
		private ScheduleProjectionHandler target;
		private MockRepository mocks;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleProjectionReadOnlyRepository = mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			target = new ScheduleProjectionHandler(unitOfWorkFactory,scheduleProjectionReadOnlyRepository);
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(unitOfWork.PersistAll());
			}
			using (mocks.Playback())
			{
				target.Handle(new ProjectionChangedEvent
				               	{
				               		IsDefaultScenario = true,
				               		PersonId = person.Id.GetValueOrDefault(),
				               		ScheduleDays = new[]
				               		               	{
				               		               		new ProjectionChangedEventScheduleDay
				               		               			{
				               		               				StartDateTime = period.StartDateTime,
				               		               				EndDateTime = period.EndDateTime,
				               		               				Layers =
				               		               					new Collection<ProjectionChangedEventLayer>
				               		               						{new ProjectionChangedEventLayer()}
				               		               			}
				               		               	}
				               	});
			}
		}

		[Test]
		public void ShouldSkipDeleteWhenDenormalizeProjectionGivenThatOptionIsSet()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(unitOfWork.PersistAll());
			}
			using (mocks.Playback())
			{
				target.Handle(new ProjectionChangedEvent
				               	{
				               		IsDefaultScenario = true,
				               		PersonId = person.Id.GetValueOrDefault(),
				               		ScheduleDays = new[]
				               		               	{
				               		               		new ProjectionChangedEventScheduleDay
				               		               			{
				               		               				StartDateTime = period.StartDateTime,
				               		               				EndDateTime = period.EndDateTime,
				               		               				Layers =
				               		               					new Collection<ProjectionChangedEventLayer>
				               		               						{new ProjectionChangedEventLayer()}
				               		               			}
				               		               	},
				               		IsInitialLoad = true,
				               	});
			}
		}

		[Test]
		public void ShouldNotDenormalizeProjectionForOtherThanDefaultScenario()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.Handle(new ProjectionChangedEvent
				               	{
				               		IsDefaultScenario = false,
				               		PersonId = person.Id.GetValueOrDefault(),
				               		ScheduleDays = new[]
				               		               	{
				               		               		new ProjectionChangedEventScheduleDay
				               		               			{
				               		               				StartDateTime = period.StartDateTime,
				               		               				EndDateTime = period.EndDateTime,
				               		               				Layers =
				               		               					new Collection<ProjectionChangedEventLayer>
				               		               						{new ProjectionChangedEventLayer()}
				               		               			}
				               		               	}
				               	});
			}
		}
	}
}