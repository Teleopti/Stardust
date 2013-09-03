using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionConsumerTest
	{
		private ScheduleProjectionReadOnlyUpdater target;
		private MockRepository mocks;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleProjectionReadOnlyRepository = mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();
			target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, MockRepository.GenerateMock<IPublishEventsFromEventHandlers>());
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.Handle(new ScheduledResourcesChangedEvent
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
				Expect.Call(()=>scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(new DateOnlyPeriod(), Guid.Empty,
				                                                                      person.Id.GetValueOrDefault()))
				      .IgnoreArguments()
				      .Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Handle(new ScheduledResourcesChangedEvent
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
				target.Handle(new ScheduledResourcesChangedEvent
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
			var serviceBus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
			target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, serviceBus);
			var guid = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(guid);

			var utcNow = DateTime.UtcNow;
			var closestPeriod = new DateTimePeriod(utcNow.AddMinutes(-5), utcNow.AddMinutes(5));
			var notClosestPeriod = new DateTimePeriod(utcNow.AddMinutes(5), utcNow.AddMinutes(10));

			var message = new ScheduledResourcesChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = guid,
					PersonId = person.Id.GetValueOrDefault(),
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									Label = "ClosestLayer",
									Date = utcNow.Date,
									Layers = new Collection<ProjectionChangedEventLayer>
										{
											new ProjectionChangedEventLayer
												{
													StartDateTime = closestPeriod.StartDateTime,
													EndDateTime = closestPeriod.EndDateTime,
												},
											new ProjectionChangedEventLayer
												{
													StartDateTime = notClosestPeriod.StartDateTime,
													EndDateTime = notClosestPeriod.EndDateTime,

												}
										}
								}
						},
					Timestamp = utcNow
				};

			target.Handle(message);

			serviceBus.AssertWasCalled(s => s.Publish(new ScheduleProjectionReadOnlyChanged
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