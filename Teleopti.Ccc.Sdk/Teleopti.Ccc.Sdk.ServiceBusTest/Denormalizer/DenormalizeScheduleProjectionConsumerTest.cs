using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
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
		private DenormalizeScheduleProjectionConsumer target;
		private MockRepository mocks;
		private IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel;
		private IScenarioRepository scenarioRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IPersonRepository personRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			updateScheduleProjectionReadModel = mocks.DynamicMock<IUpdateScheduleProjectionReadModel>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			target = new DenormalizeScheduleProjectionConsumer(unitOfWorkFactory,scenarioRepository,personRepository,updateScheduleProjectionReadModel);
		}

		[Test]
		public void ShouldDenormalizeProjection()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
				Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
				Expect.Call(()=>updateScheduleProjectionReadModel.Execute(scenario,period,person));
			}
			using (mocks.Playback())
			{
				target.Consume(new DenormalizeScheduleProjection
				{
					ScenarioId = scenario.Id.GetValueOrDefault(),
					PersonId = person.Id.GetValueOrDefault(),
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime
				});
			}
		}

		[Test]
		public void ShouldSkipDeleteWhenDenormalizeProjectionGivenThatOptionIsSet()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
				Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
				Expect.Call(() => updateScheduleProjectionReadModel.Execute(scenario, period, person));
				Expect.Call(() => updateScheduleProjectionReadModel.SetSkipDelete(true));
			}
			using (mocks.Playback())
			{
				target.Consume(new DenormalizeScheduleProjection
				{
					ScenarioId = scenario.Id.GetValueOrDefault(),
					PersonId = person.Id.GetValueOrDefault(),
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					SkipDelete = true
				});
			}
		}

		[Test]
		public void ShouldNotDenormalizeProjectionForOtherThanDefaultScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = false;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
				Expect.Call(() => updateScheduleProjectionReadModel.Execute(scenario, period, person)).Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Consume(new DenormalizeScheduleProjection
				{
					ScenarioId = scenario.Id.GetValueOrDefault(),
					PersonId = person.Id.GetValueOrDefault(),
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime
				});
			}
		}
	}
}