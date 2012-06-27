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
    public class UpdateGroupingReadModelGroupPageConsumerTest
    {
        private UpdateGroupingReadModelGroupPageConsumer _target;
        private MockRepository _mocks;
        private IGroupingReadOnlyRepository _groupReadOnlyRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();
            _target = new UpdateGroupingReadModelGroupPageConsumer(_groupReadOnlyRepository);
        }

        [Test]
        public void GroupingReadModelGroupPageTest()
        {
            //const string ids = "IDS";
            Guid TempGuid = Guid.NewGuid();

            Guid[] ids = new Guid[] { TempGuid };

            var message = new GroupPageChangedMessage();
            message.SetGroupPageIdCollection( ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelGroupPage(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(message);
            }
        }

       
    }
}