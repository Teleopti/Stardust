using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.RTA;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.RTA
{
    [TestFixture]
    public class UpdatedScheduleInfoConsumerTest
    {
        private MockRepository mocks;
        private IServiceBus serviceBus;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IUnitOfWork unitOfWork;
        private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
        private UpdatedScheduleInfoConsumer target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            serviceBus = mocks.DynamicMock<IServiceBus>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(unitOfWorkFactory);
            //mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();

            target = new UpdatedScheduleInfoConsumer(serviceBus, scheduleProjectionReadOnlyRepository, unitOfWorkFactory);
        }

        [Test]
        public void IsPersonWithExternalLogonConsumeSuccessfully()
        {
            var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

            var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
            bussinessUnit.SetId(Guid.NewGuid());
            
            var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var personInfoMessage = new PersonWithExternalLogon();
            personInfoMessage.PersonId = person.Id.GetValueOrDefault();
            personInfoMessage.Timestamp = period.StartDateTime;
            personInfoMessage.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
            personInfoMessage.Datasource = "DS";

            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);

            mocks.ReplayAll();
            target.Consume(personInfoMessage);
            mocks.VerifyAll();
        }

        [Test]
        public void DontSendDelayMessageIfThereIsNoNextActivity()
        {
            
        }

        [Test]
        public void SendDelayMessageIfThereExistNextActivity()
        {
            
        }

        [Test]
        public void IsUpdatedScheduleDayConsumeSuccessfully()
        {
           
        }

        public void DontSendAnyMessageIfTheScheduleIsNotUpdatedWithinRange()
        {
            
        }

        public void SendMessageIfTheScheduleIsNotUpdatedWithinRange()
        {
            
        }
    }
}
