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
    public class UpdateGroupingReadModelDataConsumerTest
    {
        private UpdateGroupingReadModelDataConsumer  _target;
        private MockRepository _mocks;
        private IGroupingReadOnlyRepository _groupReadOnlyRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();
            _target = new UpdateGroupingReadModelDataConsumer(_groupReadOnlyRepository);
        }


        [Test]
        public void GroupingReadModelDataTest()
        {
            var skillTest = SkillFactory.CreateSkill("Test3");
            Guid tempGuid = Guid.NewGuid();
            skillTest.SetId(tempGuid);

            Guid[] ids = new Guid[] { tempGuid };
            var message = new PersonPeriodChangedMessage();
            message.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelData(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(message);
            }
        }
    }
}