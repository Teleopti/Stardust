using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for AssignmentRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class PersonAbsenceRepositoryTest : RepositoryTest<IPersonAbsence>
    {
        private IPersonAbsenceRepository rep;
        private IAbsence dummyAbsence;
        private IPerson dummyAgent;
        private IPerson dummyAgent2;
        private IScenario dummyScenario;
        private IScenario dummyScenario2;
        private IPersonAbsence agAbsValid;

        #region Override methods

        /// <summary>
        /// Runs once a test
        /// </summary>
        protected override void ConcreteSetup()
        {
        }


        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPersonAbsence CreateAggregateWithCorrectBusinessUnit()
        {
            rep = new PersonAbsenceRepository(UnitOfWork);
            dummyAbsence = AbsenceFactory.CreateAbsence("Sjuk");
            PersistAndRemoveFromUnitOfWork(dummyAbsence);
            dummyAgent = PersonFactory.CreatePerson("h");
            dummyAgent2 = PersonFactory.CreatePerson("i"); 
            dummyScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
            dummyScenario2 = ScenarioFactory.CreateScenarioAggregate("Scenario2", false);
            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(dummyScenario);
            PersistAndRemoveFromUnitOfWork(dummyScenario2);
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 1, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 1, 17, 15, 0, DateTimeKind.Utc));
            IAbsence abs1 = AbsenceFactory.CreateAbsence("Semester");
            PersistAndRemoveFromUnitOfWork(abs1);
            AbsenceLayer layer1 = new AbsenceLayer(abs1, period1);
            agAbsValid = new PersonAbsence(dummyAgent, dummyScenario, layer1);

            return agAbsValid;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPersonAbsence loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase.Person);
            Assert.IsNotNull(loadedAggregateFromDatabase.Scenario);
            Assert.IsNotNull(loadedAggregateFromDatabase.Scenario.Id);
            Assert.IsNotNull(loadedAggregateFromDatabase.Layer);
            Assert.IsNotNull(loadedAggregateFromDatabase.Person.Id);
        }

        #endregion

        [Test]
        public void VerifyLoadGraphById()
        {
            IPersonAbsence personAbsence = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(personAbsence);

            IPersonAbsence loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(personAbsence.Id.Value);
            Assert.AreEqual(personAbsence.Id, loaded.Id);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.Layer.Payload));
        }

        [Test]
        public void VerifyLoadGraphByIdReturnsNullIfNotExists()
        {
            IPersonAbsence loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
            Assert.IsNull(loaded);
        }

        [Test]
        public void VerifyLastChange()
        {
            IPersonAbsence personAbsence = CreateAggregateWithCorrectBusinessUnit();
            DateTime? date = new DateTime(2008, 11, 17, 10, 10, 10, DateTimeKind.Utc);
            personAbsence.LastChange = date;
            PersistAndRemoveFromUnitOfWork(personAbsence);

            IPersonAbsence loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(personAbsence.Id.Value);
            Assert.AreEqual(date.Value, loaded.LastChange.Value);
        }

        [Test]
        public void VerifyAbsenceCannotBeReadForDeletedPerson()
        {
            IPersonAbsence personAbsence = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(personAbsence);
            new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Remove(personAbsence.Person);
            PersistAndRemoveFromUnitOfWork(personAbsence.Person);

            Assert.AreEqual(0, rep.Find(new DateTimePeriod(1900,1,1,2111,1,1), dummyScenario).Count);
        }

        /// <summary>
        /// Verifies that updated by and updated on is set when object is changed
        /// </summary>
        [Test]
        public void VerifyUpdatedByAndUpdatedOnIsChangedWhenObjectIsUpdated()
        {
            DateTimePeriod period = new DateTimePeriod(2007, 8, 1, 2007, 8, 2);
            IAbsence absence = AbsenceFactory.CreateAbsence("Test");
            PersistAndRemoveFromUnitOfWork(absence);
            IPerson aTinyAgent = PersonFactory.CreatePerson("Nisse");
            IScenario scen = ScenarioFactory.CreateScenarioAggregate();
            PersistAndRemoveFromUnitOfWork(aTinyAgent);
            PersistAndRemoveFromUnitOfWork(scen);
            IAbsenceLayer layer = new AbsenceLayer(absence, period);
            IPersonAbsence agAbs = new PersonAbsence(aTinyAgent, scen, layer);
            PersistAndRemoveFromUnitOfWork(agAbs);
            Assert.AreEqual(agAbs.UpdatedOn, agAbs.UpdatedOn);
        }


        [Test]
        public void VerifyCanReadAllPersonAbsencesForOnePersonAndAbsence()
        {
            ////////////setup////////////////////////////////////////////////////////////////

            IAbsence selectedAbsence = AbsenceFactory.CreateAbsence("Selected Absence");
            IAbsence notSelectedAbsence = AbsenceFactory.CreateAbsence("Not Selected");

            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 5, 2, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 5, 6, 17, 15, 0, DateTimeKind.Utc)); 
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 8, 20, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 21, 17, 15, 0, DateTimeKind.Utc));

            DateTimePeriod period4 =
              new DateTimePeriod(new DateTime(2007, 3, 10, 10, 15, 0, DateTimeKind.Utc),
                                 new DateTime(2007, 3, 13, 15, 15, 0, DateTimeKind.Utc));

            DateTimePeriod period5 =
              new DateTimePeriod(new DateTime(2007, 3, 20, 10, 15, 0, DateTimeKind.Utc),
                                 new DateTime(2007, 3, 21, 17, 15, 0, DateTimeKind.Utc));


            //persist for guid
            PersistAndRemoveFromUnitOfWork(selectedAbsence);
            PersistAndRemoveFromUnitOfWork(notSelectedAbsence);

            AbsenceLayer layer1 = new AbsenceLayer(selectedAbsence, period1);
            AbsenceLayer layer2 = new AbsenceLayer(selectedAbsence, period2);
            AbsenceLayer layer3 = new AbsenceLayer(selectedAbsence, period3);
            AbsenceLayer layer4 = new AbsenceLayer(notSelectedAbsence, period4);
            AbsenceLayer layer5 = new AbsenceLayer(selectedAbsence, period5);

            PersonAbsence agAbsValid1 = new PersonAbsence(dummyAgent, dummyScenario, layer1);
            PersonAbsence agAbsValid2 = new PersonAbsence(dummyAgent, dummyScenario, layer2);
            PersonAbsence agAbsInValid1 = new PersonAbsence(dummyAgent2, dummyScenario, layer3); 
            PersonAbsence agAbsInValid2 = new PersonAbsence(dummyAgent, dummyScenario, layer4); 
            PersonAbsence agAbsInValid3 = new PersonAbsence(dummyAgent, dummyScenario2, layer5); 

            PersistAndRemoveFromUnitOfWork(agAbsValid1);
            PersistAndRemoveFromUnitOfWork(agAbsValid2);
            PersistAndRemoveFromUnitOfWork(agAbsInValid1);
            PersistAndRemoveFromUnitOfWork(agAbsInValid2);
            PersistAndRemoveFromUnitOfWork(agAbsInValid3);

            DateTimePeriod searchPeriod =
                new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            /////////////////////////////////////////////////////////////////////////////////

            ICollection<DateTimePeriod> retList = rep.AffectedPeriods(dummyAgent, dummyScenario, searchPeriod, selectedAbsence);

           
            Assert.IsTrue(retList.Contains(period1));
            Assert.IsTrue(retList.Contains(period2));
            Assert.IsFalse(retList.Contains(period3),"invalid person");
            Assert.IsFalse(retList.Contains(period4),"invalid absence");
            Assert.IsFalse(retList.Contains(period5),"invalid scenario");
            Assert.AreEqual(2, retList.Count);
        }


        /// <summary>
        /// Verifies find assignments based on dates and agents.
        /// </summary>
        [Test]
        public void CanFindAssignmentsByDatesAndAgents()
        {
            ////////////setup////////////////////////////////////////////////////////////////
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 1, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 1, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 8, 2, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 2, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 8, 20, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 21, 17, 15, 0, DateTimeKind.Utc));
            IAbsence abs1 = AbsenceFactory.CreateAbsence("Semester");
            IAbsence abs2 = AbsenceFactory.CreateAbsence("Sjuk");
            IAbsence abs3 = AbsenceFactory.CreateAbsence("Läkare");
            PersistAndRemoveFromUnitOfWork(abs1);
            PersistAndRemoveFromUnitOfWork(abs2);
            PersistAndRemoveFromUnitOfWork(abs3);
            AbsenceLayer layer1 = new AbsenceLayer(abs1, period1);
            AbsenceLayer layer2 = new AbsenceLayer(abs2, period2);
            AbsenceLayer layer3 = new AbsenceLayer(abs3, period3);
            
            PersonAbsence agAbsValid1 = new PersonAbsence(dummyAgent, dummyScenario, layer1);
            PersonAbsence agAbsValid2 = new PersonAbsence(dummyAgent, dummyScenario, layer2);
            PersonAbsence agAbsValid3 = new PersonAbsence(dummyAgent, dummyScenario, layer3);
            PersonAbsence agAbsValid4 = new PersonAbsence(dummyAgent2, dummyScenario, layer1);

            PersistAndRemoveFromUnitOfWork(agAbsValid1);
            PersistAndRemoveFromUnitOfWork(agAbsValid2);
            PersistAndRemoveFromUnitOfWork(agAbsValid3);
            PersistAndRemoveFromUnitOfWork(agAbsValid4);

            IList<IPerson> agList = new List<IPerson>();
            agList.Add(dummyAgent);
            //agList.Add(dummyAgent2);

            DateTimePeriod searchPeriod =
                new DateTimePeriod(new DateTime(2007, 8, 1, 9, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 2, 10, 30, 0, DateTimeKind.Utc));

            /////////////////////////////////////////////////////////////////////////////////

            ICollection<IPersonAbsence> retList = rep.Find(agList, searchPeriod);

            verifyRelatedObjectsAreEagerlyLoaded(retList);
            Assert.IsTrue(retList.Contains(agAbsValid1));
            Assert.IsTrue(retList.Contains(agAbsValid2));
            Assert.AreEqual(2, retList.Count);
        }


        /// <summary>
        /// List of agents must not be null when calling find by agent and period.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void AgentListMustNotBeNullWhenCallingFindByAgentAndPeriod()
        {
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            rep.Find(null, period);
        }

        [Test]
        public void CanFindAgentAbsencesWithCorrectScenario()
        {
            Scenario okScenario = new Scenario("Low");
            Scenario noScenario = new Scenario("High");
            IAbsence abs1 = AbsenceFactory.CreateAbsence("Semester");
            IAbsence abs2 = AbsenceFactory.CreateAbsence("Sick");
            dummyAgent = PersonFactory.CreatePerson("j");
            DateTimePeriod period = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            PersistAndRemoveFromUnitOfWork(abs1);
            PersistAndRemoveFromUnitOfWork(abs2);
            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(okScenario);
            PersistAndRemoveFromUnitOfWork(noScenario);
            AbsenceLayer layer1 = new AbsenceLayer(abs1, period);
            AbsenceLayer layer2 = new AbsenceLayer(abs2, period);
            PersonAbsence personAbsence1 = new PersonAbsence(dummyAgent, noScenario, layer1);
            PersonAbsence personAbsence2 = new PersonAbsence(dummyAgent, okScenario, layer2);
            PersistAndRemoveFromUnitOfWork(personAbsence1);
            PersistAndRemoveFromUnitOfWork(personAbsence2);


            ICollection<IPersonAbsence> retList = rep.Find(new DateTimePeriod(2000, 1, 1, 2007, 1, 3), okScenario);
            verifyRelatedObjectsAreEagerlyLoaded(retList);
            Assert.AreEqual(1, retList.Count);
        }

        /// <summary>
        /// Determines whether this instance [can find agent absences with correct scenario and priod].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        public void CanFindAgentAbsencesWithCorrectScenarioAndPeriod()
        {
            Scenario okScenario = new Scenario("Low");
            Scenario noScenario = new Scenario("High");
            IAbsence abs1 = AbsenceFactory.CreateAbsence("Semester");
            IAbsence abs2 = AbsenceFactory.CreateAbsence("Sick");
            dummyAgent = PersonFactory.CreatePerson("k");
            dummyAgent2 = PersonFactory.CreatePerson("l");
            DateTimePeriod period = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            
            PersistAndRemoveFromUnitOfWork(abs1);
            PersistAndRemoveFromUnitOfWork(abs2);
            PersistAndRemoveFromUnitOfWork(dummyAgent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(okScenario);
            PersistAndRemoveFromUnitOfWork(noScenario);

            AbsenceLayer layer1 = new AbsenceLayer(abs1, period);
            AbsenceLayer layer2 = new AbsenceLayer(abs2, period);
            PersonAbsence personAbsence1 = new PersonAbsence(dummyAgent, noScenario, layer1);
            PersonAbsence personAbsence2 = new PersonAbsence(dummyAgent2, okScenario, layer2);
            
            PersistAndRemoveFromUnitOfWork(personAbsence1);
            PersistAndRemoveFromUnitOfWork(personAbsence2);

            IList<IPerson> persons = new List<IPerson>();
            persons.Add(dummyAgent2);
            ICollection<IPersonAbsence> retList = rep.Find(persons, new DateTimePeriod(2000, 1, 1, 2007, 1, 3), okScenario);
            verifyRelatedObjectsAreEagerlyLoaded(retList);
            Assert.AreEqual(1, retList.Count);
        }


		[Test]
		public void ShouldNotFetchIfScenarioIsNotLoggedOnBusinessUnit()
		{
			var absence = new Absence{Description = new Description("sdf")};
			PersistAndRemoveFromUnitOfWork(absence);
			var bu = new BusinessUnit("wrong bu");
			PersistAndRemoveFromUnitOfWork(bu);
			var scenarioWrongBu = new Scenario("wrong");
			scenarioWrongBu.SetBusinessUnit(bu);
			PersistAndRemoveFromUnitOfWork(scenarioWrongBu);
			var ass = new PersonAbsence(dummyAgent, scenarioWrongBu,
																	new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			PersistAndRemoveFromUnitOfWork(ass);

			rep.Find(new DateTimePeriod(1900, 1, 1, 2100, 1, 1), scenarioWrongBu).Should().Be.Empty();
		}

        private static void verifyRelatedObjectsAreEagerlyLoaded(IEnumerable<IPersonAbsence> personAbsenceCollection)
        {
            foreach (PersonAbsence personAbsence in personAbsenceCollection)
            {
                Assert.IsTrue(LazyLoadingManager.IsInitialized(personAbsence.Layer.Payload));
            }
        }

        protected override Repository<IPersonAbsence> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PersonAbsenceRepository(currentUnitOfWork);
        }
    }
}
