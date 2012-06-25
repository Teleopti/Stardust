using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class UpdateGroupingReadModelConsumerTest
    {
        private UpdateGroupingReadModelConsumer _target;
        private MockRepository _mocks;
        private IGroupingReadOnlyRepository _groupReadOnlyRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();
            _target = new UpdateGroupingReadModelConsumer( _groupReadOnlyRepository);
        }

        [Test]
        public void GroupingReadModelTest()
        {
            var person = PersonFactory.CreatePerson();
            Guid tempGuid = Guid.NewGuid();
            person.SetId(tempGuid);

            Guid[] ids = new Guid[] {tempGuid};
            var mess = new PersonChangedMessage();
            mess.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModel(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
        
           Assert.That(mess.Identity, Is.Not.Null);
        }

        [Test]
        public void GroupingReadModelTestWithManyIds()
        {
            var person = PersonFactory.CreatePerson();
            Guid tempGuid = Guid.NewGuid();
            person.SetId(tempGuid);

            Guid[] ids = new Guid[70];
            for(int i= 0;i<70;i++)
            {
                ids[i] = Guid.NewGuid();
            }
            var mess = new PersonChangedMessage();
            mess.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModel(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }

            Assert.That(mess.Identity, Is.Not.Null);
        }

        
    }
}