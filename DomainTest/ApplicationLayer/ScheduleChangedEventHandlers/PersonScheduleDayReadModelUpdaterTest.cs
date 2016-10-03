﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class PersonScheduleDayReadModelUpdaterTest
	{
		[Test]
		public void ShouldUpdateDayOffData()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
				{
					PersonId = personRepository.Single().Id.Value,
					ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									DayOff = new ProjectionChangedEventDayOff(),
									Name = "Day off"
								}
						}
				});

			var model = new NewtonsoftJsonSerializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.DayOff.Title.Should().Be("Day off");
		}

		[Test]
		public void ShouldNotUpdateDayOffIfNoDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									DayOff = null,
									Name = "Late"
								}
						}
			});

			var model = new NewtonsoftJsonSerializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.DayOff.Should().Be.Null();
		}

		[Test]
		public void ShouldUpdateStartAndEndFromDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									DayOff = new ProjectionChangedEventDayOff
										{
											StartDateTime = new DateTime(2013, 10, 08, 0, 0, 0),
											EndDateTime = new DateTime(2013, 10, 09, 0, 0, 0)
										},
								}
						}
			});

			var readModel = repository.Updated.Single();
			readModel.Start.Should().Be.EqualTo(new DateTime(2013, 10, 08, 0, 0, 0));
			readModel.End.Should().Be.EqualTo(new DateTime(2013, 10, 09, 0, 0, 0));
			var model = new NewtonsoftJsonSerializer().DeserializeObject<Model>(readModel.Model);
			model.DayOff.Start.Should().Be.EqualTo(new DateTime(2013, 10, 08, 0, 0, 0));
			model.DayOff.End.Should().Be.EqualTo(new DateTime(2013, 10, 09, 0, 0, 0));
		}

		[Test]
		public void ShouldUpdateFullDayAbsence()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									IsFullDayAbsence = true
								}
						}
			});

			var model = new NewtonsoftJsonSerializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.Shift.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldSendTrackingMessage()
		{
			var repository = MockRepository.GenerateMock<IPersonScheduleDayReadModelPersister>();
			var eventTracker = MockRepository.GenerateMock<ITrackingMessageSender>();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(null, repository, eventTracker);

			var initiatorId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			var @event = new ProjectionChangedEvent
			{
				InitiatorId = initiatorId,
				LogOnBusinessUnitId = businessUnitId,
				CommandId = trackId
			};
			target.Handle(@event);

			eventTracker.AssertWasCalled(x => x.SendTrackingMessage(
				Arg<ProjectionChangedEvent>.Matches(e => e.InitiatorId == initiatorId && e.LogOnBusinessUnitId == businessUnitId), 
				Arg<TrackingMessage>.Matches(m => m.TrackId == trackId)));

		}



		[Test]
		public void ShouldUpdateIsDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									DayOff = new ProjectionChangedEventDayOff(),
									Name = "Day off"
								}
						}
			});

			repository.Updated.Single().IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateIsDayOffToFalseIfNoDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
						{
							new ProjectionChangedEventScheduleDay
								{
									DayOff = null,
									Name = "Late"
								}
						}
			});

			repository.Updated.Single().IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldHaveScheduleLoadTimestampInReadModel()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var target = new PersonScheduleDayReadModelUpdaterHangfire(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);

			var scheduleLoadTimestamp = DateTime.UtcNow;

			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personRepository.Single().Id.Value,
				ScheduleDays = new[]
				{
					new ProjectionChangedEventScheduleDay
					{
						DayOff = new ProjectionChangedEventDayOff(),
						Name = "Day off"
					}
				},
				ScheduleLoadTimestamp = scheduleLoadTimestamp

			});

			var readModel = repository.Updated.Single();
			readModel.ScheduleLoadTimestamp.Should().Be.EqualTo(scheduleLoadTimestamp);
		}

	}
}