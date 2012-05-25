
using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class DenormalizeGroupingMessageConsumerTest
    {
        private DenormalizeGroupingMessageConsumer  target;
        private MockRepository mocks;
        private IUpdateGroupingReadModel _updateGroupingReadModel;
       

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _updateGroupingReadModel = mocks.DynamicMock<IUpdateGroupingReadModel>();
               target = new DenormalizeGroupingMessageConsumer( _updateGroupingReadModel);
        }

        [Test]
        public void GroupingReadModelTest()
        {
            
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            
            var ids = person.Id.ToString();

            using (mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(1, ids));
            }
            using (mocks.Playback())
            {
                target.Consume(new DenormalizeGroupingMessage()  
                {
                    Ids  = ids,
                    GroupingType   = 1
                    
                });
            }
        }

        [Test]
        public void GroupingReadModelGroupPageTest()
        {

            var groupPage = mocks.DynamicMock<IGroupPage>();
            groupPage.SetId(Guid.NewGuid());

           
            var ids = groupPage.Id.ToString();

            using (mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(2, ids));
            }
            using (mocks.Playback())
            {
                target.Consume(new DenormalizeGroupingMessage()
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

            using (mocks.Record())
            {
                Expect.Call(() => _updateGroupingReadModel.Execute(3, ids));
            }
            using (mocks.Playback())
            {
                target.Consume(new DenormalizeGroupingMessage()
                {
                    Ids = ids,
                    GroupingType = 3

                });
            }
        }
    }
}