using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class DenormalizeGroupingMessageConsumerTest
    {
        private DenormalizeGroupingMessageConsumer  _target;
        private MockRepository _mocks;
        private IUpdateGroupingReadModel _updateGroupingReadModel;
       

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _updateGroupingReadModel = _mocks.DynamicMock<IUpdateGroupingReadModel>();
            _target = new DenormalizeGroupingMessageConsumer( _updateGroupingReadModel);
        }

        [Test]
        public void GroupingReadModelTest()
        {
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            var ids = person.Id.ToString();

            using (_mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(1, ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(new DenormalizeGroupingMessage  
                {
                    Ids  = ids,
                    GroupingType   = 1
                    
                });
            }
        }

        [Test]
        public void GroupingReadModelGroupPageTest()
        {
            var groupPage = _mocks.DynamicMock<IGroupPage>();
            groupPage.SetId(Guid.NewGuid());
 
            var ids = groupPage.Id.ToString();

            using (_mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(2, ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(new DenormalizeGroupingMessage
                {
                    Ids = ids,
                    GroupingType = 2

                });
            }
        }

        [Test]
        public void GroupingReadModelDataTest()
        {
            var skillTest = SkillFactory.CreateSkill("Test3");
            skillTest.SetId(Guid.NewGuid());

            var ids = skillTest.Id.ToString();

            using (_mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(3, ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(new DenormalizeGroupingMessage
                {
                    Ids = ids,
                    GroupingType = 3

                });
            }
        }
    }
}