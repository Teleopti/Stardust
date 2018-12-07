using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class PersonFinderReadOnlyRepositoryTest : DatabaseTest
	{
		private IPersonFinderReadOnlyRepository _target;
		private Guid team1Id;
		private Guid team2Id;
		private readonly Guid personIdForFindByIdentities = Guid.NewGuid();
		private readonly Guid personIdForExternalLogon = Guid.NewGuid();
		private readonly string employmentNumber = "98765";
		private readonly string employmentNumberInOtherBu = "56789";
		private readonly string userCode = "0019";

		[SetUp]
		public void SetUp()
		{
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		protected override void SetupForRepositoryTest()
		{
			createAndSaveReadModel();
			UnitOfWork.PersistAll();
			CleanUpAfterTest();
		}
		
		[Test]
		public void ShouldNotLoadPersonsInGivenTeamsWithMultipleConflictingSimpleCriteria()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "Ashley Pierre", 10,
				new DateOnly(2016, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.FindInTeams(crit, new[] { team1Id, team2Id });
			Assert.That(crit.TotalRows, Is.EqualTo(0));
		}

		[Test]
		public void ShouldMatchMultipleValuesForOneSearchType()
		{
			var personToTest = PersonFactory.CreatePerson("Ashley","Andeen");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2011, 1, 1),
				personContract,
				team);
			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			var personToTest2 = PersonFactory.CreatePerson("Pierre","Baldi");
			PersistAndRemoveFromUnitOfWork(personToTest2);
			
			var personPeriod2 = new PersonPeriod(new DateOnly(2011, 1, 1),
				personContract,
				team);
			personToTest2.AddPersonPeriod(personPeriod2);
			PersistAndRemoveFromUnitOfWork(personToTest2);

			_target.UpdateFindPerson(new[] { personToTest.Id.Value, personToTest2.Id.Value });

			var crit = new PersonFinderSearchCriteria(PersonFinderField.FirstName, "Ashley Pierre", 10,
				new DateOnly(2016, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(2));
		}

		[Test]
		public void ShouldMatchAllValuesInAllCriteria()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "Ashley Agent", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

		[Test]
		public void ShouldLoadPersonsWithOneCriteria()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "agent", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(2));
		}

		[Test]
		public void ShouldLoadPersonsWithMultipleCriteria()
		{
			var criterias = new Dictionary<PersonFinderField, string>();
			criterias.Add(PersonFinderField.Skill, "Email");
			criterias.Add(PersonFinderField.Role, "Agent");
			var crit = new PersonFinderSearchCriteria(criterias, 10,
				new DateOnly(2020, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

		[Test]
		public void ShouldLoadPersonsCorrectBelongsToDate()
		{
			var criterias = new Dictionary<PersonFinderField, string>();
			criterias.Add(PersonFinderField.Skill, "OldSkill");
			var crit = new PersonFinderSearchCriteria(criterias, 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 6, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));

			crit = new PersonFinderSearchCriteria(criterias, 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2010, 6, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}
		
		[Test]
		public void ShouldLoadPersonsWithOneWordQuotation()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "\"Team Preference\" \"London\"", 10,
				new DateOnly(2016, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(2));
		}

		[Test]
		public void ShouldLoadPersonsByTeamContainingApostrophe()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "\"X'mas\"", 10,
				new DateOnly(2016, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}
		
		[Test]
		public void ShouldLoadPersonsWithQuotationForOneWord()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "\"Ashley\"\"Agent\"", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

		[Test]
		public void ShouldCallUpdateReadModelWithoutCrash()
		{
			_target.UpdateFindPerson(new[] { Guid.NewGuid() });
		}

		[Test]
		public void ShouldTreatEachSearchFieldAsAnd()
		{
			var buid = CurrentBusinessUnit.Make().Current().Id.GetValueOrDefault();
			team1Id = Guid.NewGuid();
			team2Id = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Autuori','108460','',NULL,'Anna', 'B0E35119-4661-4A1B-8772-9B5E015C2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Autuori','108460','',NULL,'Autuori', 'B0E35119-4661-4A1B-8772-9B5E015D2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Arianna', 'B0E35119-4661-4A1B-8772-9B5E015E2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Sanna', 'B0E35119-4661-4A1B-8772-9B5E015F2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();

			var personFinderSearchCriteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, "Anna Autuori", 10, DateOnly.Today, 0,0);
			_target.FindPeople(personFinderSearchCriteria);
			personFinderSearchCriteria.TotalRows.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldTreatEachSearchFieldAsAndTakeTwo()
		{
			var buid = CurrentBusinessUnit.Make().Current().Id.GetValueOrDefault();
			team1Id = Guid.NewGuid();
			team2Id = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Lutuori','108460','',NULL,'Anna', 'B0E35119-4661-4A1B-8772-9B5E015C2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Lutuori','108460','',NULL,'Lutuori', 'B0E35119-4661-4A1B-8772-9B5E015D2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Arianna', 'B0E35119-4661-4A1B-8772-9B5E015E2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Sanna', 'B0E35119-4661-4A1B-8772-9B5E015F2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();

			var personFinderSearchCriteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, "Anna L", 10, DateOnly.Today, 0, 0);
			_target.FindPeople(personFinderSearchCriteria);
			personFinderSearchCriteria.TotalRows.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFindAllBasedOnOneWord()
		{
			var buid = CurrentBusinessUnit.Make().Current().Id.GetValueOrDefault();
			team1Id = Guid.NewGuid();
			team2Id = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Autuori','108460','',NULL,'Anna', 'B0E35119-4661-4A1B-8772-9B5E015C2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Anna','Autuori','108460','',NULL,'Autuori', 'B0E35119-4661-4A1B-8772-9B5E015D2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Arianna', 'B0E35119-4661-4A1B-8772-9B5E015E2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
					" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Arianna','Sanna','110654','',NULL,'Sanna', 'B0E35119-4661-4A1B-8772-9B5E015F2564','LastName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();

			var personFinderSearchCriteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, "Anna", 10, DateOnly.Today, 0,0);
			_target.FindPeople(personFinderSearchCriteria);
			personFinderSearchCriteria.TotalRows.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
		{
			_target.UpdateFindPersonData(new[] { Guid.NewGuid() });
		}

		[Test]
		public void ShouldHandleTooSmallDate()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "hejhej", 10,
				new DateOnly(1012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));
		}

		[Test]
		public void ShouldFindPersonFromFindPersonModel()
		{
			var result = _target.FindPersonByIdentities(new[] { employmentNumber });
			result.First().PersonId.Should().Be.EqualTo(personIdForFindByIdentities);
			result.First().LogonName.Should().Be.EqualTo(employmentNumber);
			result.First().MatchField.Should().Be.EqualTo(IdentityMatchField.EmploymentNumber);
		}

		[Test]
		public void ShouldFindPersonFromExternalLogonModel()
		{
			var result = _target.FindPersonByIdentities(new[] { userCode });
			result.First().PersonId.Should().Be.EqualTo(personIdForExternalLogon);
			result.First().LogonName.Should().Be.EqualTo(userCode);
			result.First().MatchField.Should().Be.EqualTo(IdentityMatchField.ExternalLogon);
		}

		[Test]
		public void ShouldNotFindPersonFromFindPersonModelWhenNotInSameBusinessUnit()
		{
			var result = _target.FindPersonByIdentities(new[] { employmentNumberInOtherBu, userCode });
			result.First().PersonId.Should().Be.EqualTo(personIdForExternalLogon);
			result.First().LogonName.Should().Be.EqualTo(userCode);
			result.First().MatchField.Should().Be.EqualTo(IdentityMatchField.ExternalLogon);
		}

		private ISkill[] createAndPersistSkills()
		{
			var type = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(type);

			var activity = new Activity("test");
			PersistAndRemoveFromUnitOfWork(activity);

			var phone = SkillFactory.CreateSkill("Phone", type, 15);
			phone.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			phone.Activity = activity;
			PersistAndRemoveFromUnitOfWork(phone);

			var chat = SkillFactory.CreateSkill("Chat", type, 15);
			chat.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			chat.Activity = activity;
			PersistAndRemoveFromUnitOfWork(chat);

			var email = SkillFactory.CreateSkill("Email", type, 15);
			email.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			email.Activity = activity;
			PersistAndRemoveFromUnitOfWork(email);


			return new[] { phone, chat, email };
		}

		private void createAndSaveReadModel()
		{
			var buid = CurrentBusinessUnit.Make().Current().Id.GetValueOrDefault();
			team1Id = Guid.NewGuid();
			team2Id = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				 " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Pierre', 'B0E35119-4661-4A1B-8772-9B5E015B2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Baldi','B0E35119-4661-4A1B-8772-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Team Preferences London','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Agent','B0E35119-4661-4A1B-8772-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Email','B0E35119-4661-4A1B-8772-9B5E015B2564','Skill', :teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team1Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'137545','11610FE4-0130-4568-97DE-9B5E015B2564','EmploymentNumber',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Ashley','11610FE4-0130-4568-97DE-9B5E015B2564','FirstName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Andeen','11610FE4-0130-4568-97DE-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Team Preferences London','11610FE4-0130-4568-97DE-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Agent','11610FE4-0130-4568-97DE-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E05412741','Ashley Pierre','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E05412741','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'OldSkill','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2010, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2010, 12, 31))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", team2Id)
				.ExecuteUpdate();

			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('C0E35119-4661-4A1B-8772-9B5E015B2564','Petter','Bay','137567','',NULL,'X''mas Team','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", Guid.NewGuid())
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId", Guid.NewGuid())
				.ExecuteUpdate();


			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
					" Values (:personId,'Ashley','Andeen',:employmentNumber,'',NULL,'Team Preferences London','11610FE4-0130-4568-97DE-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("personId", personIdForFindByIdentities)
				.SetString("employmentNumber", employmentNumber)
				.SetDateTime("startDateTime", new DateTime(2017, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", Guid.NewGuid())
				.SetGuid("siteId", Guid.NewGuid())
				.SetGuid("personPeriodTeamId", Guid.NewGuid())
				.ExecuteUpdate();
			Session.CreateSQLQuery(
					"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
					" Values (:personId,'Ashley','Andeen',:employmentNumber,'',NULL,'Team Preferences London','11610FE4-0130-4568-97DE-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("personId", Guid.NewGuid())
				.SetString("employmentNumber", employmentNumberInOtherBu)
				.SetDateTime("startDateTime", new DateTime(2017, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", Guid.NewGuid())
				.SetGuid("teamId", Guid.NewGuid())
				.SetGuid("siteId", Guid.NewGuid())
				.SetGuid("personPeriodTeamId", Guid.NewGuid())
				.ExecuteUpdate();

			Session.CreateSQLQuery(
					"Insert into [ReadModel].[ExternalLogon](PersonId,DataSourceId,UserCode,Deleted,Added)" +
					"Values(:personId,'1',:userCode,0,0)")
				.SetGuid("personId", personIdForExternalLogon)
				.SetString("userCode", userCode)
				.ExecuteUpdate();
		}
	}
}