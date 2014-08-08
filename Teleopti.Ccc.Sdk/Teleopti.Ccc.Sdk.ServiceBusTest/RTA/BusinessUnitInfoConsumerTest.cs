using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
    [TestFixture]
    public class BusinessUnitInfoConsumerTest
    {
        private MockRepository mocks;
		private IBusinessUnitRepository businessUnitRepository;
	    private ICurrentUnitOfWorkFactory unitOfWorkFactory;
        private IServiceBus serviceBus;
        private BusinessUnitInfoConsumer target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();
	        unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
            serviceBus = mocks.DynamicMock<IServiceBus>();
	        target = new BusinessUnitInfoConsumer(serviceBus, unitOfWorkFactory, businessUnitRepository);
        }

		[Test]
		public void IsConsumerCalled()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

			var message = new StartUpBusinessUnit();
			message.Timestamp = period.StartDateTime;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();

			var personList = new List<Guid> { person.Id.GetValueOrDefault() };
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();

			Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			Expect.Call(loggedOnUnitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(businessUnitRepository.LoadAllPersonsWithExternalLogOn(Guid.Empty, DateOnly.Today)).IgnoreArguments().Return(personList);

			mocks.ReplayAll();
			target.Consume(message);
			mocks.VerifyAll();

		}
    }
}
