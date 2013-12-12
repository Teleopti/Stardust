using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
    [TestFixture]
    public class BusinessUnitInfoConsumerTest
    {
        private MockRepository mocks;
		private IBusinessUnitRepository businessUnitRepository;
	    private IUnitOfWorkFactory unitOfWorkFactory;
        private IServiceBus serviceBus;
        private BusinessUnitInfoConsumer target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();
	        unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            serviceBus = mocks.DynamicMock<IServiceBus>();
			target = new BusinessUnitInfoConsumer(serviceBus, unitOfWorkFactory);
        }

		//[Test]
		//public void IsConsumerCalled()
		//{
		//	var person = PersonFactory.CreatePerson();
		//	person.SetId(Guid.NewGuid());

		//	var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
		//	bussinessUnit.SetId(Guid.NewGuid());
            
		//	var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

		//	var message = new BusinessUnitInfo();
		//	message.Timestamp = period.StartDateTime;
		//	message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();

		//	var personList = new List<Guid> {person.Id.GetValueOrDefault()};
		//	var uow = mocks.DynamicMock<IUnitOfWork>();

		//	Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
		//	Expect.Call(businessUnitRepository.LoadAllPersonsWithExternalLogOn(Guid.Empty, DateOnly.Today)).IgnoreArguments().Return(personList);
            
		//	mocks.ReplayAll();
		//	target.Consume(message);
		//	mocks.VerifyAll();

		//}
    }
}
