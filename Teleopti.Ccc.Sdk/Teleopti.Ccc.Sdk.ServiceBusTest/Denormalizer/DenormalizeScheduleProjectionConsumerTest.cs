using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionConsumerTest
	{
		private ScheduleProjectionReadOnlyUpdater target;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
	    private readonly DateTime utcNow = DateTime.UtcNow;
	    private readonly DateOnly today = DateOnly.Today;
	    private IPublishEventsFromEventHandlers serviceBus;

	    [SetUp]
		public void Setup()
		{
			scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
	        serviceBus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();
	        target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, serviceBus);
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(utcNow,utcNow);

				target.Handle(new ScheduledResourcesChangedEvent
				               	{
				               		IsDefaultScenario = true,
				               		PersonId = person.Id.GetValueOrDefault(),
				               		ScheduleDays = new[]
				               		               	{
				               		               		new ProjectionChangedEventScheduleDay
				               		               			{
                                                                Date = today,
				               		               				StartDateTime = period.StartDateTime,
				               		               				EndDateTime = period.EndDateTime,
				               		               				Layers =
				               		               					new Collection<ProjectionChangedEventLayer>
				               		               						{new ProjectionChangedEventLayer()}
				               		               			}
				               		               	}
				               	});

			scheduleProjectionReadOnlyRepository.AssertWasCalled(x => x.ClearPeriodForPerson(new DateOnlyPeriod(today,today),Guid.Empty,person.Id.GetValueOrDefault()));
			serviceBus.AssertWasCalled(x => x.Publish(null), o=> o.IgnoreArguments());
		}

		[Test]
		public void ShouldSkipDeleteWhenDenormalizeProjectionGivenThatOptionIsSet()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(utcNow, utcNow);

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

		    scheduleProjectionReadOnlyRepository.AssertWasNotCalled(x => x.ClearPeriodForPerson(new DateOnlyPeriod(), Guid.Empty, person.Id.GetValueOrDefault()));
            serviceBus.AssertWasNotCalled(x => x.Publish(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotDenormalizeProjectionForOtherThanDefaultScenario()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(utcNow, utcNow);

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

		[Test]
		public void ShouldSendUpdatedScheduleDay()
		{
			target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, serviceBus);
			var guid = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(guid);

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
									Date = today,
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