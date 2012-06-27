using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class UpdateFindPersonDataConsumerTest
	{
        private MockRepository _mocks;
        private UpdateFindPersonDataConsumer  _target;
         private IPersonFinderReadOnlyRepository _finderReadOnlyRep;
       
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
             _finderReadOnlyRep = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();

             _target = new UpdateFindPersonDataConsumer(_finderReadOnlyRep);
        }

        [Test]
        public void ShouldDenormalizeNotPersons()
        {
            var skill = SkillFactory.CreateSkill("TestID");
            Guid tempGuid = Guid.NewGuid();
            skill.SetId(tempGuid);

            var ids = new[] { tempGuid };
            var mess = new PersonPeriodChangedMessage();
            mess.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _finderReadOnlyRep.UpdateFindPersonData(ids));
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
        }
	}

}