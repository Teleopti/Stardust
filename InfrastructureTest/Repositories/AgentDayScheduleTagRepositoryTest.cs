using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class AgentDayScheduleTagRepositoryTest : RepositoryTest<IAgentDayScheduleTag>
    {
        private IAgentDayScheduleTagRepository _repository;

        protected override IAgentDayScheduleTag CreateAggregateWithCorrectBusinessUnit()
        {
            var personToCreate = PersonFactory.CreatePerson("Kalle");
            var scenario = new Scenario("Active");
            var dateOnly = new DateOnly(2010, 4, 1);
            var scheduleTag = new ScheduleTag {Description = "description"};
            PersistAndRemoveFromUnitOfWork(personToCreate);
            PersistAndRemoveFromUnitOfWork(scenario);
            PersistAndRemoveFromUnitOfWork(scheduleTag);

            IAgentDayScheduleTag agentDayScheduleTag = new AgentDayScheduleTag(personToCreate, dateOnly, scenario, scheduleTag);

            return agentDayScheduleTag;
        }

        protected override void VerifyAggregateGraphProperties(IAgentDayScheduleTag loadedAggregateFromDatabase)
        {
            if (loadedAggregateFromDatabase != null)
            {
                IAgentDayScheduleTag org = CreateAggregateWithCorrectBusinessUnit();
                Assert.AreEqual(org.Scenario.Description, loadedAggregateFromDatabase.Scenario.Description);
                Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
                Assert.AreEqual(org.ScheduleTag.Description, loadedAggregateFromDatabase.ScheduleTag.Description);
                Assert.AreEqual(org.TagDate, loadedAggregateFromDatabase.TagDate);
            }
        }

        protected override Repository<IAgentDayScheduleTag> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new AgentDayScheduleTagRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyFindByPeriodAndScenario()
        {
            IAgentDayScheduleTag tag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(tag);
            _repository = new AgentDayScheduleTagRepository(UnitOfWork);
            Assert.AreEqual(1, _repository.Find(new DateTimePeriod(2010, 4, 1, 2010, 4, 2), tag.Scenario).Count);
        }

        [Test]
        public void VerifyDoNotFindByPeriodAndScenario()
        {
            IAgentDayScheduleTag tag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(tag);
            _repository = new AgentDayScheduleTagRepository(UnitOfWork);
            Assert.AreEqual(0, _repository.Find(new DateTimePeriod(2010, 4, 14, 2010, 4, 21), tag.Scenario).Count);
        }

        [Test]
        public void VerifyFindByPeriodPersonsAndScenario()
        {
            IAgentDayScheduleTag tag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(tag);
            _repository = new AgentDayScheduleTagRepository(UnitOfWork);
            ICollection<IPerson> persons = new List<IPerson> { tag.Person };
            Assert.AreEqual(1, _repository.Find(new DateOnlyPeriod(2010, 4, 1, 2010, 4, 2), persons, tag.Scenario).Count);
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            IAgentDayScheduleTag tag = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(tag);
            _repository = new AgentDayScheduleTagRepository(UnitOfWork);
            Assert.IsNotNull(tag.Id);
            Assert.AreEqual(tag, _repository.LoadAggregate(tag.Id.Value));
        }

    }
}
