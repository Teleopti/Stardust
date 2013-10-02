using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;

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
									IsDayOff = true,
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
									IsDayOff = false,
									Name = "Late"
								}
						}
			});

			var model = new NewtonsoftJsonDeserializer().DeserializeObject<Model>(repository.Updated.Single().Model);
			model.DayOff.Should().Be.Null();
		}

	}
}