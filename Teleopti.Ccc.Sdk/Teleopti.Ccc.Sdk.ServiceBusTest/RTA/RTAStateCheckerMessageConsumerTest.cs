using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.RTA;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.RTA
{
    [TestFixture]
    public class RTAStateCheckerMessageConsumerTest
    {
        private MockRepository mocks;
        private IStatisticRepository statisticRepository;
        private IServiceBus serviceBus;
        private RTAStateCheckerMessageConsumer target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            statisticRepository = mocks.DynamicMock<IStatisticRepository>();
            serviceBus = mocks.DynamicMock<IServiceBus>();
            target = new RTAStateCheckerMessageConsumer(statisticRepository, serviceBus);
        }

        [Test]
        public void IsConsumerCalled()
        {
            var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

            var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
            bussinessUnit.SetId(Guid.NewGuid());
            
            var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);

            var message = new RTAStateCheckerMessage();
            message.Timestamp = period.StartDateTime;
            message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();

            var personList = new List<Guid>();
            personList.Add(person.Id.GetValueOrDefault());

            Expect.Call(statisticRepository.PersonIdsWithExternalLogOn()).Return(personList);
            
            mocks.ReplayAll();
            target.Consume(message);
            mocks.VerifyAll();

        }
    }
}
