using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class DenormalizePersonFinderConsumerTest
    {
        private DenormalizePersonFinderConsumer _target;
        private MockRepository _mocks;
        private IUpdatePersonFinderReadModel _updatePersonFinderReadModel;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _updatePersonFinderReadModel = _mocks.DynamicMock<IUpdatePersonFinderReadModel>();
             _target = new DenormalizePersonFinderConsumer(_updatePersonFinderReadModel);
        }

        [Test]
        public void ShouldDenormalizePersons()
        {        
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

			var ids = person.Id.ToString();

            using (_mocks.Record())
            {
                 Expect.Call(() => _updatePersonFinderReadModel.Execute(true,ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(new DenormalizePersonFinderMessage
                {
                    Ids  = ids,
                    IsPerson  = true
                    
                });
            }
        }

        [Test]
        public void ShouldDenormalizeNotPersons()
        {
            var skill = SkillFactory.CreateSkill("TestID");
            skill.SetId(Guid.NewGuid());

            var ids = skill.Id.ToString();

            using (_mocks.Record())
            {
                Expect.Call(() => _updatePersonFinderReadModel.Execute(false, ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(new DenormalizePersonFinderMessage
                {
                    Ids = ids,
                    IsPerson = false
                });
            }
        }
    }
}