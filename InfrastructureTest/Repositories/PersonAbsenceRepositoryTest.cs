using System;
using System.Collections.Generic;
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
        private IAbsence absenceSick;
        private IPerson agent;
        private IScenario defaultScenario;

        #region Override methods

        /// <summary>
        /// Runs once a test
        /// </summary>
        protected override void ConcreteSetup()
        {
			absenceSick = AbsenceFactory.CreateAbsence("Sjuk");
			PersistAndRemoveFromUnitOfWork(absenceSick);
			agent = PersonFactory.CreatePerson("h");
			defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
			PersistAndRemoveFromUnitOfWork(agent);
			PersistAndRemoveFromUnitOfWork(defaultScenario);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPersonAbsence CreateAggregateWithCorrectBusinessUnit()
        {
            var period1 = new DateTimePeriod(new DateTime(2007, 8, 1, 10, 15, 0, DateTimeKind.Utc), new DateTime(2007, 8, 1, 17, 15, 0, DateTimeKind.Utc));
            var layer1 = new AbsenceLayer(absenceSick, period1);
            return new PersonAbsence(agent, defaultScenario, layer1);
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
            var personAbsence = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(personAbsence);

            var loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(personAbsence.Id.Value);
            Assert.AreEqual(personAbsence.Id, loaded.Id);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.Layer.Payload));
        }

        [Test]
        public void VerifyLoadGraphByIdReturnsNullIfNotExists()
        {
            var loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
            Assert.IsNull(loaded);
        }

        [Test]
        public void VerifyLastChange()
        {
            var personAbsence = CreateAggregateWithCorrectBusinessUnit();
            DateTime? date = new DateTime(2008, 11, 17, 10, 10, 10, DateTimeKind.Utc);
            personAbsence.LastChange = date;
            PersistAndRemoveFromUnitOfWork(personAbsence);

            IPersonAbsence loaded = new PersonAbsenceRepository(UnitOfWork).LoadAggregate(personAbsence.Id.Value);
            Assert.AreEqual(date.Value, loaded.LastChange.Value);
        }

        [Test]
        public void VerifyAbsenceCannotBeReadForDeletedPerson()
        {
            var personAbsence = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(personAbsence);
	        var currentUnitOfWork = new ThisUnitOfWork(UnitOfWork);
	        new PersonRepository(currentUnitOfWork).Remove(personAbsence.Person);
            PersistAndRemoveFromUnitOfWork(personAbsence.Person);

			Assert.AreEqual(0, new PersonAbsenceRepository(UnitOfWork).Find(new DateTimePeriod(1900, 1, 1, 2111, 1, 1), defaultScenario).Count);
        }

        [Test]
        public void VerifyUpdatedByAndUpdatedOnIsChangedWhenObjectIsUpdated()
        {
			IPersonAbsence personAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personAbsence);
	        
            Assert.IsTrue(personAbsence.UpdatedOn.HasValue);
        }

		[Test]
		public void ShouldIgnoreOtherAbsencesWhenLoadingPeriodsForAbsence()
		{
			IAbsence notSelectedAbsence = AbsenceFactory.CreateAbsence("Not Selected");
			PersistAndRemoveFromUnitOfWork(notSelectedAbsence);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));
			
			AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period1);
			AbsenceLayer layer2 = new AbsenceLayer(notSelectedAbsence, period2);

			PersonAbsence agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			PersonAbsence agAbsInValid2 = new PersonAbsence(agent, defaultScenario, layer2);
			
			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);
			
			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);

			var retList = new PersonAbsenceRepository(UnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsFalse(retList.Contains(period2), "invalid absence");
			Assert.AreEqual(1, retList.Count);
		}

		[Test]
		public void ShouldIncludeOtherAbsencesWhenLoadingPeriodsForAbsenceGivenSpecificAbsence()
		{
			var absenceVacation = AbsenceFactory.CreateAbsence("Vacation");
			PersistAndRemoveFromUnitOfWork(absenceVacation);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));

			var layer1 = new AbsenceLayer(absenceSick, period1);
			var layer2 = new AbsenceLayer(absenceVacation, period2);

			var personAbsence1 = new PersonAbsence(agent, defaultScenario, layer1);
			var personAbsence2 = new PersonAbsence(agent, defaultScenario, layer2);

			PersistAndRemoveFromUnitOfWork(personAbsence1);
			PersistAndRemoveFromUnitOfWork(personAbsence2);

			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
			var retList = new PersonAbsenceRepository(UnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsTrue(retList.Contains(period2));
			Assert.AreEqual(2, retList.Count);
		}

		[Test]
		public void ShouldIgnoreOtherScenariosWhenLoadingPeriodsForAbsence()
		{
			var noScenario = new Scenario("High");
			PersistAndRemoveFromUnitOfWork(noScenario);
            
			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));

			AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period1);
			AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period2);

			PersonAbsence agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			PersonAbsence agAbsInValid2 = new PersonAbsence(agent, noScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
			var retList = new PersonAbsenceRepository(UnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsFalse(retList.Contains(period2), "invalid scenario");
			Assert.AreEqual(1, retList.Count);
		}

		[Test]
		public void ShouldIgnoreOtherAgentsWhenLoadingPeriodsForAbsence()
		{
			var dummyAgent2 = PersonFactory.CreatePerson("i");
			PersistAndRemoveFromUnitOfWork(dummyAgent2);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));

			AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period1);
			AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period2);

			PersonAbsence agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			PersonAbsence agAbsInValid2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
			var retList = new PersonAbsenceRepository(UnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsFalse(retList.Contains(period2), "invalid scenario");
			Assert.AreEqual(1, retList.Count);
		}

		[Test]
		public void ShouldIgnoreAbsenceNotIntersectingGivenPeriodOrOtherAbsenceWhenLoadingPeriodsForAbsence()
		{
			var dummyAgent2 = PersonFactory.CreatePerson("i");
			PersistAndRemoveFromUnitOfWork(dummyAgent2);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(7));

			AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period1);
			AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period2);

			PersonAbsence agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			PersonAbsence agAbsInValid2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var retList = new PersonAbsenceRepository(UnitOfWork).AffectedPeriods(agent, defaultScenario, period1, absenceSick);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsFalse(retList.Contains(period2), "invalid period");
			Assert.AreEqual(1, retList.Count);
		}

        [Test]
        public void CanFindAssignmentsByDatesAndAgents()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 1, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 1, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 8, 2, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 2, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 8, 20, 10, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 21, 17, 15, 0, DateTimeKind.Utc));
            
			var dummyAgent2 = PersonFactory.CreatePerson("i");
			PersistAndRemoveFromUnitOfWork(dummyAgent2);
			
            AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period1);
            AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period2);
            AbsenceLayer layer3 = new AbsenceLayer(absenceSick, period3);
            
            PersonAbsence agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
            PersonAbsence agAbsValid2 = new PersonAbsence(agent, defaultScenario, layer2);
            PersonAbsence agAbsValid3 = new PersonAbsence(agent, defaultScenario, layer3);
            PersonAbsence agAbsValid4 = new PersonAbsence(dummyAgent2, defaultScenario, layer1);

            PersistAndRemoveFromUnitOfWork(agAbsValid1);
            PersistAndRemoveFromUnitOfWork(agAbsValid2);
            PersistAndRemoveFromUnitOfWork(agAbsValid3);
            PersistAndRemoveFromUnitOfWork(agAbsValid4);

            var agList = new List<IPerson> {agent};
	        var searchPeriod =
                new DateTimePeriod(new DateTime(2007, 8, 1, 9, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 2, 10, 30, 0, DateTimeKind.Utc));

			var retList = new PersonAbsenceRepository(UnitOfWork).Find(agList, searchPeriod);

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
			new PersonAbsenceRepository(UnitOfWork).Find(null, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
        }

        [Test]
        public void CanFindAgentAbsencesWithCorrectScenario()
        {
            Scenario noScenario = new Scenario("High");
			PersistAndRemoveFromUnitOfWork(noScenario);
            
		    DateTimePeriod period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period);
            AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period);
            PersonAbsence personAbsence1 = new PersonAbsence(agent, noScenario, layer1);
            PersonAbsence personAbsence2 = new PersonAbsence(agent, defaultScenario, layer2);
            PersistAndRemoveFromUnitOfWork(personAbsence1);
            PersistAndRemoveFromUnitOfWork(personAbsence2);

			ICollection<IPersonAbsence> retList = new PersonAbsenceRepository(UnitOfWork).Find(new DateTimePeriod(2000, 1, 1, 2007, 1, 3), defaultScenario);
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
            Scenario noScenario = new Scenario("High");
            var agent = PersonFactory.CreatePerson("k");
            var dummyAgent2 = PersonFactory.CreatePerson("l");
            DateTimePeriod period = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            
            PersistAndRemoveFromUnitOfWork(agent);
            PersistAndRemoveFromUnitOfWork(dummyAgent2);
            PersistAndRemoveFromUnitOfWork(noScenario);

            AbsenceLayer layer1 = new AbsenceLayer(absenceSick, period);
            AbsenceLayer layer2 = new AbsenceLayer(absenceSick, period);
            PersonAbsence personAbsence1 = new PersonAbsence(agent, noScenario, layer1);
            PersonAbsence personAbsence2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);
            
            PersistAndRemoveFromUnitOfWork(personAbsence1);
            PersistAndRemoveFromUnitOfWork(personAbsence2);

            IList<IPerson> persons = new List<IPerson>();
            persons.Add(dummyAgent2);
			ICollection<IPersonAbsence> retList = new PersonAbsenceRepository(UnitOfWork).Find(persons, new DateTimePeriod(2000, 1, 1, 2007, 1, 3), defaultScenario);
            verifyRelatedObjectsAreEagerlyLoaded(retList);
            Assert.AreEqual(1, retList.Count);
        }


		[Test]
		public void ShouldNotFetchIfScenarioIsNotLoggedOnBusinessUnit()
		{
			var bu = new BusinessUnit("wrong bu");
			PersistAndRemoveFromUnitOfWork(bu);
			var scenarioWrongBu = new Scenario("wrong");
			scenarioWrongBu.SetBusinessUnit(bu);
			PersistAndRemoveFromUnitOfWork(scenarioWrongBu);
			var ass = new PersonAbsence(agent, scenarioWrongBu,
																	new AbsenceLayer(absenceSick, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			PersistAndRemoveFromUnitOfWork(ass);

			new PersonAbsenceRepository(UnitOfWork).Find(new DateTimePeriod(1900, 1, 1, 2100, 1, 1), scenarioWrongBu).Should().Be.Empty();
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
