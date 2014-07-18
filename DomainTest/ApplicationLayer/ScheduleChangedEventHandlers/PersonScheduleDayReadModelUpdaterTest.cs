using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class PersonScheduleDayReadModelUpdaterTest
	{
		[Test]
		public void ShouldUpdateDayOffData()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepository();
			var target = new PersonScheduleDayReadModelUpdater(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository);

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

			var model = new NewtonsoftJsonDeserializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.DayOff.Title.Should().Be("Day off");
		}

		[Test]
		public void ShouldNotUpdateDayOffIfNoDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepository();
			var target = new PersonScheduleDayReadModelUpdater(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository);

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

			var model = new NewtonsoftJsonDeserializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.DayOff.Should().Be.Null();
		}

		[Test]
		public void ShouldUpdateStartAndEndFromDayOff()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepository();
			var target = new PersonScheduleDayReadModelUpdater(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository);

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
			var model = new NewtonsoftJsonDeserializer().DeserializeObject<Model>(readModel.Model);
			model.DayOff.Start.Should().Be.EqualTo(new DateTime(2013, 10, 08, 0, 0, 0));
			model.DayOff.End.Should().Be.EqualTo(new DateTime(2013, 10, 09, 0, 0, 0));
		}

		[Test]
		public void ShouldUpdateFullDayAbsence()
		{
			var repository = new FakePersonScheduleDayReadModelPersister();
			var personRepository = new FakePersonRepository();
			var target = new PersonScheduleDayReadModelUpdater(new PersonScheduleDayReadModelsCreator(personRepository, new NewtonsoftJsonSerializer()), repository);

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

			var model = new NewtonsoftJsonDeserializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.Shift.IsFullDayAbsence.Should().Be.True();
		}


		[Test]
		public void ShouldUpdateIfPersonTerminated()
		{
			var repository = MockRepository.GenerateMock<IPersonScheduleDayReadModelPersister>();
			var target = new PersonScheduleDayReadModelUpdater(null, repository);

			var terminationDate = new DateTime(2000, 10, 31);
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			target.Handle(new PersonTerminatedEvent
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				TerminationDate = terminationDate
			});

			repository.AssertWasCalled(
				x =>
					x.UpdateReadModels(new DateOnlyPeriod(new DateOnly(terminationDate), DateOnly.MaxValue), personId, businessUnitId,
						null, false));
		}

	}
}