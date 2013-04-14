using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionConsumerTest
	{
		private ScheduleProjectionHandler target;
		private MockRepository mocks;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
	    private IServiceBus serviceBus;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleProjectionReadOnlyRepository = mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();
			target = new ScheduleProjectionHandler(scheduleProjectionReadOnlyRepository);
			target = new DenormalizeScheduleProjectionConsumer(unitOfWorkFactory,scheduleProjectionReadOnlyRepository, serviceBus);
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
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
		public void ShouldDenormalizeProjectionToo()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.Handle(new ProjectionChangedEventForScheduleProjection
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
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
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
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(mocks.DynamicMock<IUnitOfWorkFactory>());
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

		[Test]
		public void ShouldSendUpdatedScheduleDay()
		{
			var _serviceBus = MockRepository.GenerateMock<IServiceBus>();
			target = new DenormalizeScheduleProjectionConsumer(unitOfWorkFactory, scheduleProjectionReadOnlyRepository, _serviceBus);
			var guid = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(guid);

			var utcNow = DateTime.UtcNow;
			var closestPeriod = new DateTimePeriod(utcNow.AddMinutes(-5), utcNow.AddMinutes(5));
			var notClosestPeriod = new DateTimePeriod(utcNow.AddMinutes(5), utcNow.AddMinutes(10));

			var message = new DenormalizedSchedule
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = guid,
					PersonId = person.Id.GetValueOrDefault(),
					ScheduleDays = new[]
						{
							new DenormalizedScheduleDay
								{
									Label = "ClosestLayer",
									Date = utcNow.Date,
									Layers = new Collection<DenormalizedScheduleProjectionLayer>
										{
											new DenormalizedScheduleProjectionLayer
												{
													StartDateTime = closestPeriod.StartDateTime,
													EndDateTime = closestPeriod.EndDateTime,
												},
											new DenormalizedScheduleProjectionLayer
												{
													StartDateTime = notClosestPeriod.StartDateTime,
													EndDateTime = notClosestPeriod.EndDateTime,

												}
										}
								}
						},
					Timestamp = utcNow
				};

			target.Consume(message);

			_serviceBus.AssertWasCalled(s => s.Send(new UpdatedScheduleDay
				{
					Datasource = message.Datasource,
					BusinessUnitId = message.BusinessUnitId,
					PersonId = message.PersonId,
					ActivityStartDateTime = closestPeriod.StartDateTime,
					ActivityEndDateTime = closestPeriod.EndDateTime,
					Timestamp = utcNow
				}), o => o.IgnoreArguments());

		}
	}
}