using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class UpdateFindPersonConsumerTest
	{
        private MockRepository _mocks;
        private UpdateFindPersonConsumer  _target;
         private IPersonFinderReadOnlyRepository _finderReadOnlyRep;
       
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
             _finderReadOnlyRep = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();

             _target = new UpdateFindPersonConsumer(_finderReadOnlyRep);
        }

        [Test]
        public void UpdateFindPersonTestCall()
        {
            var person = PersonFactory.CreatePerson();
            Guid tempGuid = Guid.NewGuid();
            person.SetId(tempGuid);

            Guid[] ids = new Guid[] { tempGuid };
            var mess = new PersonChangedMessage();
            mess.SetPersonIdCollection(ids);
            using (_mocks.Record())
            {
                Expect.Call(() => _finderReadOnlyRep.UpdateFindPerson(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
            Assert.That(mess.Identity, Is.Not.Null);
        }
	}

}