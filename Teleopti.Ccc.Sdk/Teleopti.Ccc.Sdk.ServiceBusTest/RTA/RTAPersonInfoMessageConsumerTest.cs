using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class RTAPersonInfoMessageConsumerTest
    {
        private MockRepository mocks;
        private IServiceBus serviceBus;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IUnitOfWork unitOfWork;
        private IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository;
        private RTAPersonInfoMessageConsumer target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            serviceBus = mocks.DynamicMock<IServiceBus>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(unitOfWorkFactory);
                //mocks.DynamicMock<IScheduleProjectionReadOnlyRepository>();

            target = new RTAPersonInfoMessageConsumer(serviceBus, scheduleProjectionReadOnlyRepository, unitOfWorkFactory);
        }

        [Test]
        public void IsRTAPersonInfoMessageConsumeSuccessfully()
        {
            var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

            var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
            bussinessUnit.SetId(Guid.NewGuid());
            
            var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var personInfoMessage = new RTAPersonInfoMessage();
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
        public void IsRTAUpdatedScheduleDayMessageConsumeSuccessfully()
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
