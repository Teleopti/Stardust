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
    public class DenormalizePersonFinderConsumerTest
    {
        private DenormalizePersonFinderConsumer target;
        private MockRepository mocks;
        private IUpdatePersonFinderReadModel updatePersonFinderReadModel;
        private IScenarioRepository scenarioRepository;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IPersonRepository personRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            updatePersonFinderReadModel = mocks.DynamicMock<IUpdatePersonFinderReadModel>();
            scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            personRepository = mocks.DynamicMock<IPersonRepository>();
            target = new DenormalizePersonFinderConsumer(updatePersonFinderReadModel);
        }

        [Test]
        public void ShouldDenormalizePersons()
        {
            
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
            
            var ids = person.Id.ToString();

            using (mocks.Record())
            {
                 Expect.Call(() => updatePersonFinderReadModel.Execute(true,ids));
            }
            using (mocks.Playback())
            {
                target.Consume(new DenormalizePersonFinder() 
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

            using (mocks.Record())
            {
                Expect.Call(() => updatePersonFinderReadModel.Execute(false, ids));
            }
            using (mocks.Playback())
            {
                target.Consume(new DenormalizePersonFinder()
                {
                    Ids = ids,
                    IsPerson = false

                });
            }
        }

       
    }
}