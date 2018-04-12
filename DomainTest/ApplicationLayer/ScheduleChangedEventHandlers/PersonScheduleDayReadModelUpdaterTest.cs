using System;
using System.Linq;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class PersonScheduleDayReadModelUpdaterTest
	{
		private readonly bool _toggle75415;

		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		public PersonScheduleDayReadModelUpdaterTest(bool toggle75415)
		{
			_toggle75415 = toggle75415;
		}
		
		[Test]
		public void ShouldUpdateDayOffData()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepositoryLegacy();
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
										}
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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(null, repository, eventTracker);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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
			var persister = new PersonScheduleDayReadModelUpdaterPersister(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository, null);
			var target = createTarget(persister);

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

		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		private IPersonScheduleDayReadModelUpdaterHangfire createTarget(PersonScheduleDayReadModelUpdaterPersister persister)
		{
			return _toggle75415
				? (IPersonScheduleDayReadModelUpdaterHangfire) new ScheduleReadModelWrapperHandler(null, persister)
				: new PersonScheduleDayReadModelUpdaterHangfire(persister);
		}
		
		public IComponentContext TempContainer;
		[Test]
		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		[Toggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		public void MustNotUseOldHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() =>
				TempContainer.Resolve<PersonScheduleDayReadModelUpdaterHangfire>());
		}
		
		[Test]
		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		[ToggleOff(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		public void MustNotUseNewHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() =>
				TempContainer.Resolve<ScheduleReadModelWrapperHandler>());
		}
	}

}