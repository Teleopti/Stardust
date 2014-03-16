using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionConsumerTest
	{
		private ScheduleProjectionReadOnlyUpdater target;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private IPublishEventsFromEventHandlers serviceBus;
		private Guid personId, businessUnitId;
	    private readonly DateTime utcNow = DateTime.UtcNow;
	    private readonly DateOnly today = DateOnly.Today;

	    [SetUp]
		public void Setup()
		{
			scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			serviceBus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();

			personId = Guid.NewGuid();
			businessUnitId = Guid.NewGuid();

			target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, serviceBus, new Now());
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
									Shift = new ProjectionChangedEventShift
										{
											StartDateTime = period.StartDateTime,
											EndDateTime = period.EndDateTime,
											Layers =
												new Collection<ProjectionChangedEventLayer>
													{
														new ProjectionChangedEventLayer
															{
																StartDateTime = utcNow.AddHours(-1),
																EndDateTime = utcNow.AddHours(1)
															}
													}
										}
								}
						}
				});

			scheduleProjectionReadOnlyRepository.AssertWasCalled(x => x.ClearPeriodForPerson(new DateOnlyPeriod(today,today),Guid.Empty,person.Id.GetValueOrDefault()), o => o.IgnoreArguments());
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
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									Shift = new ProjectionChangedEventShift
										{
											StartDateTime = period.StartDateTime,
											EndDateTime = period.EndDateTime,
											Layers =
												new Collection<ProjectionChangedEventLayer>
													{new ProjectionChangedEventLayer()}
										}
								}
						},
					IsInitialLoad = true,
				});

		    scheduleProjectionReadOnlyRepository.AssertWasNotCalled(x => x.ClearPeriodForPerson(new DateOnlyPeriod(), Guid.Empty, person.Id.GetValueOrDefault()));
            serviceBus.AssertWasNotCalled(x => x.Publish(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldSendUpdatedScheduleDay()
		{
			target = new ScheduleProjectionReadOnlyUpdater(scheduleProjectionReadOnlyRepository, serviceBus, new Now());
			var closestPeriod = new DateTimePeriod(utcNow.AddMinutes(-5), utcNow.AddMinutes(5));
			var notClosestPeriod = new DateTimePeriod(utcNow.AddMinutes(5), utcNow.AddMinutes(10));

			var message = new ScheduledResourcesChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									ShortName = "ClosestLayer",
									Date = today,
									Shift = new ProjectionChangedEventShift
										{
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

		[Test]
		public void Handle_ScheduleChangeForTomorrow_ShouldSendUpdatedScheduleDay()
		{
			var closestPeriod = new DateTimePeriod(utcNow.AddDays(1).AddMinutes(10), utcNow.AddDays(1).AddMinutes(20));

			var message = new ScheduledResourcesChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									ShortName = "ClosestLayer",
									Date = utcNow.Date.AddDays(1),
									Shift = new ProjectionChangedEventShift
										{
											Layers = new Collection<ProjectionChangedEventLayer>
												{
													new ProjectionChangedEventLayer
														{
															StartDateTime = closestPeriod.StartDateTime,
															EndDateTime = closestPeriod.EndDateTime,
														}
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

		[Test]
		public void Handle_SheduleChangeForYesterday_ShouldNotSend()
		{
			var closestPeriod = new DateTimePeriod(utcNow.AddDays(-2).AddMinutes(10), utcNow.AddDays(-1).AddMinutes(20));

			var message = new ScheduledResourcesChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									ShortName = "ClosestLayer",
									Date = closestPeriod.StartDateTime.Date,
									Shift = new ProjectionChangedEventShift
										{
											Layers = new Collection<ProjectionChangedEventLayer>
												{
													new ProjectionChangedEventLayer
														{
															StartDateTime = closestPeriod.StartDateTime,
															EndDateTime = closestPeriod.EndDateTime,
														},
													new ProjectionChangedEventLayer
														{
															StartDateTime = closestPeriod.StartDateTime.AddMinutes(5),
															EndDateTime = closestPeriod.EndDateTime.AddMinutes(20),
														}
												}
										}
								}
						},
					Timestamp = utcNow
				};

			target.Handle(message);

			serviceBus.AssertWasNotCalled(s => s.Publish(new ScheduleProjectionReadOnlyChanged
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				ActivityStartDateTime = closestPeriod.StartDateTime,
				ActivityEndDateTime = closestPeriod.EndDateTime,
				Timestamp = utcNow
			}), o => o.IgnoreArguments());
		}

		[Test]
		public void Handle_NoNextActivityStartTime_ShouldSend()
		{
			var message2 = new ScheduledResourcesChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									ShortName = "ClosestLayer",
									Date = utcNow.Date,
									Shift = new ProjectionChangedEventShift
										{
											Layers = new Collection<ProjectionChangedEventLayer>
												{
													new ProjectionChangedEventLayer
														{
															StartDateTime = utcNow.AddMinutes(-10),
															EndDateTime = utcNow.AddHours(1)
														}
												}
										}
								}
						},
					Timestamp = utcNow
				};

			scheduleProjectionReadOnlyRepository.Expect(s => s.GetNextActivityStartTime(utcNow, personId))
			                                    .IgnoreArguments()
			                                    .Return(null);

			var message = message2;
			target.Handle(message);
			serviceBus.AssertWasCalled(s => s.Publish(new ScheduleProjectionReadOnlyChanged
				{
					Datasource = message.Datasource,
					BusinessUnitId = message.BusinessUnitId,
					PersonId = message.PersonId,
					ActivityStartDateTime = utcNow,
					ActivityEndDateTime = utcNow,
					Timestamp = utcNow
				}), o => o.IgnoreArguments());
		}
	}
}