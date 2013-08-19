using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests PersonDayOffRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class PersonDayOffRepositoryTest : RepositoryTest<IPersonDayOff>
    {
        /// <summary>
        /// Renove this suppress later when more tests are written
        /// </summary>
        private PersonDayOffRepository rep;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            rep = new PersonDayOffRepository(UnitOfWork);
        }
        
        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPersonDayOff CreateAggregateWithCorrectBusinessUnit()
        {
            IPersonDayOff personDayOff = PersonDayOffFactory.CreatePersonDayOff();

            PersistAndRemoveFromUnitOfWork(personDayOff.Person);
            PersistAndRemoveFromUnitOfWork(personDayOff.Scenario);

            return personDayOff;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPersonDayOff loadedAggregateFromDatabase)
        {
            IPersonDayOff org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.DayOff.Anchor, loadedAggregateFromDatabase.DayOff.Anchor);
            Assert.AreEqual(org.DayOff.Flexibility,
                            loadedAggregateFromDatabase.DayOff.Flexibility);
            Assert.AreEqual(org.DayOff.TargetLength,
                            loadedAggregateFromDatabase.DayOff.TargetLength);
        }


        [Test]
        public void VerifyLoadGraphById()
        {
            IPersonDayOff personDayOff = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(personDayOff);

            IPersonDayOff loaded = new PersonDayOffRepository(UnitOfWork).LoadAggregate(personDayOff.Id.Value);
            Assert.AreEqual(personDayOff.Id, loaded.Id);
        }

        [Test]
        public void VerifyLoadGraphByIdReturnsNullIfNotExists()
        {
            IPersonDayOff loaded = new PersonDayOffRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
            Assert.IsNull(loaded);
        }

        /// <summary>
        /// Verifies find DayOffs based on agents.
        /// </summary>
        [Test]
        public void CanFindDayOffsByAgents()
        {
            IPerson dummyAgent = PersonFactory.CreatePerson("r");
            IPerson dummyAgent2 = PersonFactory.CreatePerson("s");
            IScenario dummyScenario = ScenarioFactory.CreateScenarioAggregate();

            ////////////setup////////////////////////////////////////////////////////////////

            DayOffTemplate dOff = new DayOffTemplate(new Description("test"));
            dOff.Anchor = TimeSpan.FromHours(10);
            dOff.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.Zero);
            PersonDayOff personDayOff1 = new PersonDayOff(dummyAgent, dummyScenario, dOff, new DateOnly(new DateTime(2007, 1, 1)));


            DayOffTemplate dOff1 = new DayOffTemplate(new Description("test"));
            dOff1.Anchor = TimeSpan.FromHours(3);
            dOff1.SetTargetAndFlexibility(TimeSpan.FromHours(3), TimeSpan.Zero);

            PersonDayOff personDayOff2 = new PersonDayOff(dummyAgent2, dummyScenario, dOff1, new DateOnly(new DateTime(2007, 1, 1)));
            PersonDayOff personDayOff3 = new PersonDayOff(dummyAgent2, dummyScenario, dOff, new DateOnly(new DateTime(2007, 1, 3)));

            DayOffTemplate dOff2 = new DayOffTemplate(new Description("test"));
            dOff2.Anchor = TimeSpan.FromHours(3);
            dOff2.SetTargetAndFlexibility(TimeSpan.FromHours(3), TimeSpan.Zero);

            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(dummyScenario);

            PersistAndRemoveFromUnitOfWork(dOff);
            PersistAndRemoveFromUnitOfWork(dOff1);
            
            PersistAndRemoveFromUnitOfWork(personDayOff1);
            PersistAndRemoveFromUnitOfWork(personDayOff2);
            PersistAndRemoveFromUnitOfWork(personDayOff3);

            IList<IPerson> agList = new List<IPerson>();
            agList.Add(dummyAgent);

            /////////////////////////////////////////////////////////////////////////////////

            ICollection<IPersonDayOff> retList = rep.Find(agList);

            Assert.IsTrue(retList.Contains(personDayOff1));
            Assert.IsFalse(retList.Contains(personDayOff2));
            Assert.AreEqual(1, retList.Count);
        }

        /// <summary>
        /// Verifies find DayOffs based on agents and date time period.
        /// </summary>
        [Test]
        public void CanFindDayOffsByAgentAndDateTimePeriod()
        {
            IPerson dummyAgent = PersonFactory.CreatePerson("t");
            IPerson dummyAgent2 = PersonFactory.CreatePerson("u");
            IScenario dummyScenario = ScenarioFactory.CreateScenarioAggregate();

            ////////////setup////////////////////////////////////////////////////////////////

            PersonDayOff personDayOffs1 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));
            PersonDayOff personDayOffs2 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 2, 1)));
            PersonDayOff personDayOffs3 = new PersonDayOff(dummyAgent2, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));

            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(dummyScenario);

            PersistAndRemoveFromUnitOfWork(personDayOffs1);
            PersistAndRemoveFromUnitOfWork(personDayOffs2);
            PersistAndRemoveFromUnitOfWork(personDayOffs3);

            IList<IPerson> agList = new List<IPerson>();
            agList.Add(dummyAgent);

            /////////////////////////////////////////////////////////////////////////////////
            DateTimePeriod dateTimeExpression = new DateTimePeriod(new DateTime(2006, 12, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 31, 0, 0, 0, DateTimeKind.Utc));
            ICollection<IPersonDayOff> retList = rep.Find(agList, dateTimeExpression);

            Assert.IsTrue(retList.Contains(personDayOffs1));
            Assert.IsFalse(retList.Contains(personDayOffs2));
            Assert.IsFalse(retList.Contains(personDayOffs3));
            Assert.AreEqual(1, retList.Count);
        }

        /// <summary>
        /// Determines whether this instance [can find day offs by agent and date time period and scenario].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        public void CanFindDayOffsByAgentAndDateTimePeriodAndScenario()
        {
            IPerson dummyAgent = PersonFactory.CreatePerson("v");
            IPerson dummyAgent2 = PersonFactory.CreatePerson("x");
            IScenario dummyScenario = ScenarioFactory.CreateScenarioAggregate();

            ////////////setup////////////////////////////////////////////////////////////////
            PersonDayOff personDayOffs1 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));
            PersonDayOff personDayOffs2 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 2, 1)));
            PersonDayOff personDayOffs3 = new PersonDayOff(dummyAgent2, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));
            
            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(dummyScenario);

            PersistAndRemoveFromUnitOfWork(personDayOffs1);
            PersistAndRemoveFromUnitOfWork(personDayOffs2);
            PersistAndRemoveFromUnitOfWork(personDayOffs3);

            IList<IPerson> agList = new List<IPerson>();
            agList.Add(dummyAgent);

            /////////////////////////////////////////////////////////////////////////////////
            DateTimePeriod dateTimeExpression =
                new DateTimePeriod(new DateTime(2006, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 31, 0, 0, 0, DateTimeKind.Utc));
            ICollection<IPersonDayOff> retList = rep.Find(agList, dateTimeExpression, dummyScenario);

            Assert.IsTrue(retList.Contains(personDayOffs1));
            Assert.IsFalse(retList.Contains(personDayOffs2));
            Assert.IsFalse(retList.Contains(personDayOffs3));
            Assert.AreEqual(1, retList.Count);
        }

        /// <summary>
        /// Verifies find DayOffs based on agents and date time period using UTC.
        /// </summary>
        [Test]
        public void CanFindDayOffsByAgentAndDateTimePeriodUsingUtc()
        {
            IPerson dummyAgent = PersonFactory.CreatePerson("11");
            IPerson dummyAgent2 = PersonFactory.CreatePerson("22");
            IScenario dummyScenario = ScenarioFactory.CreateScenarioAggregate();

            ////////////setup////////////////////////////////////////////////////////////////

            PersonDayOff personDayOffs1 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));
            PersonDayOff personDayOffs2 = new PersonDayOff(dummyAgent, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 2, 1)));
            PersonDayOff personDayOffs3 = new PersonDayOff(dummyAgent2, dummyScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));

            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(dummyScenario);

            PersistAndRemoveFromUnitOfWork(personDayOffs1);
            PersistAndRemoveFromUnitOfWork(personDayOffs2);
            PersistAndRemoveFromUnitOfWork(personDayOffs3);

            IList<IPerson> agList = new List<IPerson>();
            agList.Add(dummyAgent);

            /////////////////////////////////////////////////////////////////////////////////
            DateTimePeriod dateTimeExpression = new DateTimePeriod(new DateTime(2006, 12, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 31, 0, 0, 0, DateTimeKind.Utc));
            ICollection<IPersonDayOff> retList = rep.Find(agList, dateTimeExpression);

            Assert.IsTrue(retList.Contains(personDayOffs1));
            Assert.IsFalse(retList.Contains(personDayOffs2));
            Assert.IsFalse(retList.Contains(personDayOffs3));
            Assert.AreEqual(1, retList.Count);
        }

        [Test]
        public void CanFindAgentDayOffsWithCorrectScenario()
        {
            IScenario okScenario = ScenarioFactory.CreateScenarioAggregate("Low",false);
            IScenario noScenario = ScenarioFactory.CreateScenarioAggregate("High", false);
            IPerson dummyAgent = PersonFactory.CreatePerson("245");
            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(okScenario);
            PersistAndRemoveFromUnitOfWork(noScenario);

            PersonDayOff personDayOff1 = new PersonDayOff(dummyAgent, okScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 1, 1)));
            PersonDayOff personDayOff2 = new PersonDayOff(dummyAgent, noScenario, CreateDayOff(), new DateOnly(new DateTime(2007, 2, 1)));

            PersistAndRemoveFromUnitOfWork(personDayOff1);
            PersistAndRemoveFromUnitOfWork(personDayOff2);
            

            ICollection<IPersonDayOff> retList = rep.Find(new DateTimePeriod(2007,1,1, 2007,1,3), okScenario);
            Assert.AreEqual(1, retList.Count);
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            IPersonDayOffRepository personDayOffRepository = new PersonDayOffRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(personDayOffRepository);
        }

        protected override Repository<IPersonDayOff> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PersonDayOffRepository(unitOfWork);
        }

        private static DayOffTemplate CreateDayOff()
        {
            return CreateDayOff(TimeSpan.FromHours(4));
        }

        private static DayOffTemplate CreateDayOff(TimeSpan length)
        {
            DayOffTemplate dOff = new DayOffTemplate(new Description("test"));
            dOff.Anchor = TimeSpan.FromHours(10);
            dOff.SetTargetAndFlexibility(length, TimeSpan.Zero);
            
            return dOff;
        }
    }
}
