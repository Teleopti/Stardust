using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonAbsenceRepositoryTest : RepositoryTest<IPersonAbsence>
	{
		private IAbsence absenceSick;
		private IPerson agent;
		private IScenario defaultScenario;
		
		protected override void ConcreteSetup()
		{
			absenceSick = AbsenceFactory.CreateAbsence("Sjuk");
			PersistAndRemoveFromUnitOfWork(absenceSick);
			agent = PersonFactory.CreatePerson("h");
			defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
			PersistAndRemoveFromUnitOfWork(agent);
			PersistAndRemoveFromUnitOfWork(defaultScenario);
		}
		
		protected override IPersonAbsence CreateAggregateWithCorrectBusinessUnit()
		{
			var period1 = new DateTimePeriod(new DateTime(2007, 8, 1, 10, 15, 0, DateTimeKind.Utc), new DateTime(2007, 8, 1, 17, 15, 0, DateTimeKind.Utc));
			var layer1 = new AbsenceLayer(absenceSick, period1);
			return new PersonAbsence(agent, defaultScenario, layer1);
		}
		
		protected override void VerifyAggregateGraphProperties(IPersonAbsence loadedAggregateFromDatabase)
		{
			Assert.IsNotNull(loadedAggregateFromDatabase.Person);
			Assert.IsNotNull(loadedAggregateFromDatabase.Scenario);
			Assert.IsNotNull(loadedAggregateFromDatabase.Scenario.Id);
			Assert.IsNotNull(loadedAggregateFromDatabase.Layer);
			Assert.IsNotNull(loadedAggregateFromDatabase.Person.Id);
		}

		[Test]
		public void VerifyLoadGraphById()
		{
			var personAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personAbsence);

			var loaded = new PersonAbsenceRepository(CurrUnitOfWork).LoadAggregate(personAbsence.Id.Value);
			Assert.AreEqual(personAbsence.Id, loaded.Id);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(loaded.Layer.Payload));
		}

		[Test]
		public void VerifyLoadGraphByIdReturnsNullIfNotExists()
		{
			var loaded = new PersonAbsenceRepository(CurrUnitOfWork).LoadAggregate(Guid.NewGuid());
			Assert.IsNull(loaded);
		}

		[Test]
		public void VerifyLastChange()
		{
			var personAbsence = CreateAggregateWithCorrectBusinessUnit();
			DateTime? date = new DateTime(2008, 11, 17, 10, 10, 10, DateTimeKind.Utc);
			personAbsence.LastChange = date;
			PersistAndRemoveFromUnitOfWork(personAbsence);

			var loaded = new PersonAbsenceRepository(CurrUnitOfWork).LoadAggregate(personAbsence.Id.Value);
			Assert.AreEqual(date.Value, loaded.LastChange.Value);
		}

		[Test]
		public void VerifyAbsenceCannotBeReadForDeletedPerson()
		{
			var personAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personAbsence);
			new PersonRepository(CurrUnitOfWork, null, null).Remove(personAbsence.Person);
			PersistAndRemoveFromUnitOfWork(personAbsence.Person);

			Assert.AreEqual(0, new PersonAbsenceRepository(CurrUnitOfWork).Find(new DateTimePeriod(1900, 1, 1, 2111, 1, 1), defaultScenario).Count);
		}

		[Test]
		public void VerifyUpdatedByAndUpdatedOnIsChangedWhenObjectIsUpdated()
		{
			var personAbsence = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(personAbsence);
			
			Assert.IsTrue(personAbsence.UpdatedOn.HasValue);
		}

		[Test]
		public void ShouldIgnoreOtherAbsencesWhenLoadingPeriodsForAbsence()
		{
			var notSelectedAbsence = AbsenceFactory.CreateAbsence("Not Selected");
			PersistAndRemoveFromUnitOfWork(notSelectedAbsence);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));
			
			var layer1 = new AbsenceLayer(absenceSick, period1);
			var layer2 = new AbsenceLayer(notSelectedAbsence, period2);

			var agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			var agAbsInValid2 = new PersonAbsence(agent, defaultScenario, layer2);
			
			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);
			
			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);

			var retList = new PersonAbsenceRepository(CurrUnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

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
			var retList = new PersonAbsenceRepository(CurrUnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod);

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

			var layer1 = new AbsenceLayer(absenceSick, period1);
			var layer2 = new AbsenceLayer(absenceSick, period2);

			var agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			var agAbsInValid2 = new PersonAbsence(agent, noScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
			var retList = new PersonAbsenceRepository(CurrUnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

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

			var layer1 = new AbsenceLayer(absenceSick, period1);
			var layer2 = new AbsenceLayer(absenceSick, period2);

			var agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			var agAbsInValid2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var searchPeriod = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
			var retList = new PersonAbsenceRepository(CurrUnitOfWork).AffectedPeriods(agent, defaultScenario, searchPeriod, absenceSick);

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

			var layer1 = new AbsenceLayer(absenceSick, period1);
			var layer2 = new AbsenceLayer(absenceSick, period2);

			var agAbsValid1 = new PersonAbsence(agent, defaultScenario, layer1);
			var agAbsInValid2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);

			PersistAndRemoveFromUnitOfWork(agAbsValid1);
			PersistAndRemoveFromUnitOfWork(agAbsInValid2);

			var retList = new PersonAbsenceRepository(CurrUnitOfWork).AffectedPeriods(agent, defaultScenario, period1, absenceSick);

			Assert.IsTrue(retList.Contains(period1));
			Assert.IsFalse(retList.Contains(period2), "invalid period");
			Assert.AreEqual(1, retList.Count);
		}
		
		[Test]
		public void CanFindAgentAbsencesWithCorrectScenario()
		{
			var noScenario = new Scenario("High");
			PersistAndRemoveFromUnitOfWork(noScenario);
			
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var layer1 = new AbsenceLayer(absenceSick, period);
			var layer2 = new AbsenceLayer(absenceSick, period);
			var personAbsence1 = new PersonAbsence(agent, noScenario, layer1);
			var personAbsence2 = new PersonAbsence(agent, defaultScenario, layer2);
			PersistAndRemoveFromUnitOfWork(personAbsence1);
			PersistAndRemoveFromUnitOfWork(personAbsence2);

			var retList = new PersonAbsenceRepository(CurrUnitOfWork).Find(new DateTimePeriod(2000, 1, 1, 2007, 1, 3), defaultScenario);
			verifyRelatedObjectsAreEagerlyLoaded(retList);
			Assert.AreEqual(1, retList.Count);
		}

		[Test]
		public void CanFindExactPersonAbsence()
		{			

			var period1 = new DateTimePeriod(2000,1,1, 8,2000,1,1, 16);
			var period2 = new DateTimePeriod(2000,1,1,9,2000,1,1,14);
			var layer1 = new AbsenceLayer(absenceSick,period1);
			var layer2 = new AbsenceLayer(absenceSick,period2);
			var personAbsence1 = new PersonAbsence(agent,defaultScenario,layer1);
			var personAbsence2 = new PersonAbsence(agent,defaultScenario,layer2);
			PersistAndRemoveFromUnitOfWork(personAbsence1);
			PersistAndRemoveFromUnitOfWork(personAbsence2);

			var retList = new PersonAbsenceRepository(CurrUnitOfWork).FindExact(agent, period1, absenceSick, defaultScenario);
			verifyRelatedObjectsAreEagerlyLoaded(retList);
			retList.Single().Period.Should().Be.EqualTo(period1);
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
			var noScenario = new Scenario("High");
			var dummyAgent1 = PersonFactory.CreatePerson("k");
			var dummyAgent2 = PersonFactory.CreatePerson("l");
			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
								   new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc));
			
			PersistAndRemoveFromUnitOfWork(dummyAgent1);
			PersistAndRemoveFromUnitOfWork(dummyAgent2);
			PersistAndRemoveFromUnitOfWork(noScenario);

			var layer1 = new AbsenceLayer(absenceSick, period);
			var layer2 = new AbsenceLayer(absenceSick, period);
			var personAbsence1 = new PersonAbsence(dummyAgent1, noScenario, layer1);
			var personAbsence2 = new PersonAbsence(dummyAgent2, defaultScenario, layer2);
			
			PersistAndRemoveFromUnitOfWork(personAbsence1);
			PersistAndRemoveFromUnitOfWork(personAbsence2);

			IList<IPerson> persons = new List<IPerson>();
			persons.Add(dummyAgent2);
			var retList = new PersonAbsenceRepository(CurrUnitOfWork).Find(persons, new DateTimePeriod(2000, 1, 1, 2007, 1, 3), defaultScenario);
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
			var ass = new PersonAbsence(agent, scenarioWrongBu, new AbsenceLayer(absenceSick, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			PersistAndRemoveFromUnitOfWork(ass);

			new PersonAbsenceRepository(CurrUnitOfWork).Find(new DateTimePeriod(1900, 1, 1, 2100, 1, 1), scenarioWrongBu).Should().Be.Empty();
		}

		[Test]
		public void ShouldCheckIfNoAgentsScheduled()
		{
			new PersonAbsenceRepository(CurrUnitOfWork).IsThereScheduledAgents(Guid.NewGuid(), new DateOnlyPeriod()).Should().Be.False();
		}

		[TestCase(1, 1, true)] // Absence started in query period, ended in query period
		[TestCase(-1, 4, true)] // Absence started before query period, ended after query period
		[TestCase(-1, 2, true)] // Absence started before query period, ended in query period
		[TestCase(1, 4, true)] // Absence started in query period, ended after query period
		[TestCase(-3, -2, false)] // Absence started before query period, ended before query period
		[TestCase(4, 8, false)] // Absence started after query period, ended after query period
		public void ShouldCheckIfAnyAgentsScheduled(int relativeStartDate, int relativeEndDate, bool expectedResult)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var contract = new Contract("contract");
			var partTimePercentage = new PartTimePercentage("partTimePercentage");
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("contractSchedule");

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(partTimePercentage);
			PersistAndRemoveFromUnitOfWork(contractSchedule);

			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-1), new PersonContract(contract, partTimePercentage, contractSchedule), team);
			agent.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod);
			PersistAndRemoveFromUnitOfWork(agent);

			var buWithAgentScheduled = team.Site.BusinessUnit;

			var scenario = new Scenario("scenario");
			scenario.SetBusinessUnit(buWithAgentScheduled);
			PersistAndRemoveFromUnitOfWork(scenario);

			var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var absencePeriod = new DateTimePeriod(baseDate.AddDays(relativeStartDate),
				baseDate.AddDays(relativeEndDate));
			var ass = new PersonAbsence(agent, scenario, new AbsenceLayer(absenceSick, absencePeriod));
			PersistAndRemoveFromUnitOfWork(ass);

			var buWithoutAgentScheduled = new BusinessUnit("businessUnit");
			PersistAndRemoveFromUnitOfWork(buWithoutAgentScheduled);

			var period = new DateOnlyPeriod(new DateOnly(baseDate), new DateOnly(baseDate.AddDays(3)));
			new PersonAbsenceRepository(CurrUnitOfWork).IsThereScheduledAgents(buWithAgentScheduled.Id.Value, period)
				.Should().Be.EqualTo(expectedResult);
			new PersonAbsenceRepository(CurrUnitOfWork).IsThereScheduledAgents(buWithoutAgentScheduled.Id.Value, period)
				.Should().Be.False();
		}

		private static void verifyRelatedObjectsAreEagerlyLoaded(IEnumerable<IPersonAbsence> personAbsenceCollection)
		{
			foreach (var personAbsence in personAbsenceCollection)
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
