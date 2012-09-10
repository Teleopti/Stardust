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
	public class InitialLoadScheduleDayConsumerTest
	{
		private MockRepository _mocks;
		private IScenarioRepository _scenarioRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonRepository _personRepository;
		private IUnitOfWork _unitOfWork;
		private IPersonAssignmentRepository _personAssignmentRep;
		private IServiceBus _serviceBus;
		private InitialLoadScheduleDayConsumer _target;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scenarioRepository = _mocks.DynamicMock<IScenarioRepository>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_personRepository = _mocks.DynamicMock<IPersonRepository>();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();
			_personAssignmentRep = _mocks.StrictMock<IPersonAssignmentRepository>();
			_serviceBus = _mocks.StrictMock<IServiceBus>();

			_target = new InitialLoadScheduleDayConsumer(_unitOfWorkFactory, _personRepository, _scheduleDayReadModelRepository, _personAssignmentRep,
			                                             _scenarioRepository, _serviceBus);
		}

		[Test]
		public void ShouldCheckIfSavedAssignments()
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_scheduleDayReadModelRepository.IsInitialized()).Return(false);
			Expect.Call(_personAssignmentRep.CountAllEntities()).Return(0);
			Expect.Call(_unitOfWork.Clear);
			Expect.Call(_unitOfWork.Dispose);
			_mocks.ReplayAll();
			_target.Consume(new InitialLoadScheduleDay() { BusinessUnitId = Guid.NewGuid(), Datasource = "data" });
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSendMessagesToBusIfAssignments()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			scenario.SetId(Guid.NewGuid());
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_scheduleDayReadModelRepository.IsInitialized()).Return(false);
			Expect.Call(_personAssignmentRep.CountAllEntities()).Return(1);
			Expect.Call(_personRepository.LoadAll()).Return(new List<IPerson> {PersonFactory.CreatePersonWithGuid("ola", "la")});
			Expect.Call(_scenarioRepository.LoadDefaultScenario()).Return(scenario);
			Expect.Call(_unitOfWork.Clear);
			Expect.Call(_unitOfWork.Dispose);
			Expect.Call(() => _serviceBus.Send(new DenormalizeScheduleDayMessage())).IgnoreArguments();
			_mocks.ReplayAll();
			_target.Consume(new InitialLoadScheduleDay(){BusinessUnitId = Guid.NewGuid(),Datasource = "data"});
			_mocks.VerifyAll();
		}
	}


}