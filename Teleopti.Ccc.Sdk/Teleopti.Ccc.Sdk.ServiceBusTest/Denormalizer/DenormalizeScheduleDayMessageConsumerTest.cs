using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleDayMessageConsumerTest
	{
		private DenormalizeScheduleDayMessageConsumer _target;
		private MockRepository _mocks;
		private IScenarioRepository _scenarioRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonRepository _personRepository;
		private IUnitOfWork _unitOfWork;
		private IScheduleDayReadModelsCreator _readModelsCreator;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
	    private IServiceBus _serviceBus;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scenarioRepository = _mocks.DynamicMock<IScenarioRepository>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_personRepository = _mocks.DynamicMock<IPersonRepository>();
			_readModelsCreator = _mocks.DynamicMock<IScheduleDayReadModelsCreator>();
			_scheduleDayReadModelRepository = _mocks.DynamicMock<IScheduleDayReadModelRepository>();
		    _serviceBus = _mocks.DynamicMock<IServiceBus>();
			_target = new DenormalizeScheduleDayMessageConsumer(_unitOfWorkFactory,_scenarioRepository,_personRepository,_readModelsCreator,_scheduleDayReadModelRepository,_serviceBus);
		}

		[Test]
		public void ShouldDenormalizeDay()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = true;

			var person = PersonFactory.CreatePerson();
			var id = Guid.NewGuid();
			person.SetId(id);

			var date = DateTime.UtcNow;
			var period = new DateTimePeriod(date, date);
			var models = new List<ScheduleDayReadModel>{new ScheduleDayReadModel()};
			
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
			Expect.Call(_readModelsCreator.GetReadModels(scenario,period,person)).Return(models);
			Expect.Call(() => _scheduleDayReadModelRepository.SaveReadModels(models));
			Expect.Call(_unitOfWork.Dispose);
			_mocks.ReplayAll();
			_target.Consume(new DenormalizeScheduleDayMessage
			{
				ScenarioId = scenario.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime
			});
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotDenormalizeDayForOtherThanDefaultScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = false;

			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			using (_mocks.Record())
			{
				Expect.Call(_scenarioRepository.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
				Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person).Repeat.Never();
			}
			using (_mocks.Playback())
			{
				_target.Consume(new DenormalizeScheduleDayMessage
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