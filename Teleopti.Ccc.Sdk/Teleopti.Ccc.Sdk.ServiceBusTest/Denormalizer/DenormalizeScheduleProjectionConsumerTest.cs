using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
		private ScheduleProjectionHandler target;
		private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
		private IPublishEventsFromEventHandlers serviceBus;
		private Guid personId, businessUnitId;

		[SetUp]
		public void Setup()
		{
			scheduleProjectionReadOnlyRepository = MockRepository.GenerateMock<IScheduleProjectionReadOnlyRepository>();
			serviceBus = MockRepository.GenerateMock<IPublishEventsFromEventHandlers>();

			personId = Guid.NewGuid();
			businessUnitId = Guid.NewGuid();

			target = new ScheduleProjectionHandler(scheduleProjectionReadOnlyRepository, serviceBus);
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			target.Handle(new ProjectionChangedEvent
				{
					IsDefaultScenario = true,
					PersonId = personId,
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
		public void ShouldDenormalizeProjectionToo()
		{
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			target.Handle(new ProjectionChangedEventForScheduleProjection
				{
					IsDefaultScenario = true,
					PersonId = personId,
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
		public void ShouldSkipDeleteWhenDenormalizeProjectionGivenThatOptionIsSet()
		{
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			target.Handle(new ProjectionChangedEvent
				{
					IsDefaultScenario = true,
					PersonId = personId,
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

		[Test]
		public void ShouldNotDenormalizeProjectionForOtherThanDefaultScenario()
		{
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			target.Handle(new ProjectionChangedEvent
				{
					IsDefaultScenario = false,
					PersonId = personId,
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
			var utcNow = DateTime.UtcNow;
			var closestPeriod = new DateTimePeriod(utcNow.AddMinutes(-5), utcNow.AddMinutes(5));
			var notClosestPeriod = new DateTimePeriod(utcNow.AddMinutes(5), utcNow.AddMinutes(10));

			var message = new ProjectionChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
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

			serviceBus.AssertWasCalled(s => s.Publish(new UpdatedScheduleDay
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
			var utcNow = DateTime.UtcNow;
			var closestPeriod = new DateTimePeriod(utcNow.AddDays(1).AddMinutes(10), utcNow.AddDays(1).AddMinutes(20));

			var message = new ProjectionChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									Label = "ClosestLayer",
									Date = utcNow.Date.AddDays(1),
									Layers = new Collection<ProjectionChangedEventLayer>
										{
											new ProjectionChangedEventLayer
												{
													StartDateTime = closestPeriod.StartDateTime,
													EndDateTime = closestPeriod.EndDateTime,
												}
										}
								}
						},
					Timestamp = utcNow
				};

			target.Handle(message);

			serviceBus.AssertWasCalled(s => s.Publish(new UpdatedScheduleDay
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
			var utcNow = DateTime.UtcNow;
			var closestPeriod = new DateTimePeriod(utcNow.AddDays(-2).AddMinutes(10), utcNow.AddDays(-1).AddMinutes(20));

			var message = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				Datasource = "DataSource",
				BusinessUnitId = businessUnitId,
				PersonId = personId,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									Label = "ClosestLayer",
									Date = closestPeriod.StartDateTime.Date,
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
						},
				Timestamp = utcNow
			};

			target.Handle(message);

			serviceBus.AssertWasNotCalled(s => s.Publish(new UpdatedScheduleDay
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
			var utcNow = DateTime.UtcNow;

			var message2 = new ProjectionChangedEvent
				{
					IsDefaultScenario = true,
					Datasource = "DataSource",
					BusinessUnitId = businessUnitId,
					PersonId = personId,
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
													StartDateTime = utcNow.AddMinutes(-10),
													EndDateTime = utcNow.AddHours(1)
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
			serviceBus.AssertWasCalled(s => s.Publish(new UpdatedScheduleDay
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