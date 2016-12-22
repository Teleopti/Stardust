using NUnit.Framework;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonFinderReadOnlyRepositoryWithTeamsTest
	{
		public IPersonFinderReadOnlyRepository Target;
		public ISiteRepository Sites;
		public ITeamRepository Teams;
		public IPersonRepository Persons;
		public IContractRepository Contracts;
		public IContractScheduleRepository ContractSchedules;
		public IPartTimePercentageRepository PartTimePercentages;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldMatchAllValuesInGivenTeamsWithEmptyCriteria()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.Description = new Description("sdf");
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley","Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011,1,1),createPersonContract(),team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per);
			});

			createAndSaveReadModel(per.Id.Value, per.Name.FirstName, per.Name.LastName, team.Id.Value, site.Id.Value, site.BusinessUnit.Id.Value, new DateTime(2011, 1, 1));

			var crit = new PersonFinderSearchCriteria(new Dictionary<PersonFinderField,string>(),10,
				new DateOnly(2020,1,1),new Dictionary<string,bool>(),new DateOnly(2011,12,1));

			var result = WithUnitOfWork.Get(() =>
			{
				Target.FindInTeams(crit, new[] {team.Id.Value});
				return crit.TotalRows;
			});
			

			Assert.That(result,Is.EqualTo(1));
		}

		[Test]
		public void ShouldMatchAllValuesInGivenTeamsWithGivenCriteria()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.Description = new Description("sdf");
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley","Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011,1,1),createPersonContract(),team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per);
			});

			createAndSaveReadModel(per.Id.Value,per.Name.FirstName,per.Name.LastName,team.Id.Value,site.Id.Value,site.BusinessUnit.Id.Value,new DateTime(2011,1,1));

			var crit = new PersonFinderSearchCriteria(PersonFinderField.All,"Ashley",10,
				new DateOnly(2020,1,1),new Dictionary<string,bool>(),new DateOnly(2011,12,1));

			var result = WithUnitOfWork.Get(() =>
			{
				Target.FindInTeams(crit,new[] { team.Id.Value });
				return crit.TotalRows;
			});


			Assert.That(result,Is.EqualTo(1));
		}

		private IPersonContract createPersonContract(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if(otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			WithUnitOfWork.Do(() =>
			{
				Contracts.Add(pContract.Contract);
				ContractSchedules.Add(pContract.ContractSchedule);
				PartTimePercentages.Add(pContract.PartTimePercentage);
			});

			return pContract;
		}

		private void createAndSaveReadModel(Guid personId, string firstName, string lastName, Guid teamId, Guid siteId, Guid businessUnitId, DateTime startDateTime )
		{			

			WithUnitOfWork.Do(uow =>
			{			
				uow.Current().FetchSession().CreateSQLQuery(
			  "Insert into [ReadModel].[FindPerson] (PersonId,  FirstName,  LastName,  EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId                        ,SearchType        , TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
			  " Values (                            :personId, :firstName, :lastName,   '137545'           ,''  ,NULL ,  :searchValue   ,'11610FE4-0130-4568-97DE-9B5E015B2564','FirstName',:teamId, :siteId,:businessUnitId, :startDateTime, NULL)")
			  .SetGuid("personId", personId)
			  .SetString("firstName", firstName)
			  .SetString("lastName", lastName)
			  .SetString("searchValue", firstName)
			  .SetDateTime("startDateTime",startDateTime)			 
			  .SetGuid("businessUnitId",businessUnitId)
			  .SetGuid("teamId",teamId)
			  .SetGuid("siteId",siteId)			
			  .ExecuteUpdate();
			});		
		}


	}


    [TestFixture, Category("BucketB")]
    public class PersonFinderReadOnlyRepositoryTest : DatabaseTest
    {
        private IPersonFinderReadOnlyRepository _target;
	    private Guid team1Id;
	    private Guid team2Id;

		protected override void SetupForRepositoryTest()
	    {
			createAndSaveReadModel();
			UnitOfWork.PersistAll();
			CleanUpAfterTest();
	    }


		[Test]
		public void ShouldNotLoadPersonsInGivenTeamsWithMultipleConflictingSimpleCriteria()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All,"Ashley Pierre",10,
				new DateOnly(2016,1,1),new Dictionary<string,bool>(),new DateOnly(2011,12,1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.FindInTeams(crit,new[] { team1Id, team2Id });
			Assert.That(crit.TotalRows,Is.EqualTo(0));
		}

		[Test]
		public void ShouldMatchAllValuesInAllCriteria()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "Ashley Agent", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

        [Test]
        public void ShouldLoadPersonsWithOneCriteria()
        {
	        var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "agent", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
            _target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.Find(crit);
            Assert.That(crit.TotalRows, Is.EqualTo(2));
        }

		[Test]
		public void ShouldLoadPersonsWithMultipleCriteria()
		{
			var criterias = new Dictionary<PersonFinderField, string>();
			criterias.Add(PersonFinderField.FirstName,"Ashley");
			criterias.Add(PersonFinderField.Role,"Agent");
			var crit = new PersonFinderSearchCriteria(criterias, 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
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
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));

			crit = new PersonFinderSearchCriteria(criterias, 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2010, 6, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}


		[Test]
		public void ShouldLoadPersonsWithOneWordQuotation()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "\"Team Preference\" \"London\"", 10,
				new DateOnly(2016, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(2));
		}

		[Test]
		public void ShouldLoadPersonsByTeamContainingApostrophe()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All,"\"X'mas\"",10,
				new DateOnly(2016,1,1),new Dictionary<string,bool>(),new DateOnly(2011,12,1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows,Is.EqualTo(1));
		}


		[Test]
		public void ShouldLoadPersonsWithQuotationForOneWord()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "\"Ashley\"\"Agent\"", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

	    [Test]
		public void ShouldCallUpdateReadModelWithoutCrash()
		{
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.UpdateFindPerson(new[ ] {Guid.NewGuid()});
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
		{
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.UpdateFindPersonData(new[] { Guid.NewGuid() });
		}

		[Test]
		public void ShouldHandleTooSmallDate()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "hejhej", 10,
				new DateOnly(1012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));
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
				.SetGuid("businessUnitId",buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team1Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Baldi','B0E35119-4661-4A1B-8772-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team1Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Team Preferences London','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team1Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Agent','B0E35119-4661-4A1B-8772-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team1Id)
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
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Ashley','11610FE4-0130-4568-97DE-9B5E015B2564','FirstName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, personPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Andeen','11610FE4-0130-4568-97DE-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Team Preferences London','11610FE4-0130-4568-97DE-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Agent','11610FE4-0130-4568-97DE-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E05412741','Ashley Pierre','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E05412741','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'OldSkill','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetDateTime("startDateTime", new DateTime(2010, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2010, 12, 31))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.SetGuid("personPeriodTeamId",team2Id)
				.ExecuteUpdate();

			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime, PersonPeriodTeamId)" +
				" Values ('C0E35119-4661-4A1B-8772-9B5E015B2564','Petter','Bay','137567','',NULL,'X''mas Team','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime, :personPeriodTeamId)")
				.SetGuid("businessUnitId",buid)
				.SetDateTime("startDateTime",new DateTime(2011,1,1))
				.SetDateTime("endDateTime",new DateTime(2045,1,1))
				.SetGuid("teamId",Guid.NewGuid())
				.SetGuid("siteId",siteId)
				.SetGuid("personPeriodTeamId",Guid.NewGuid())
				.ExecuteUpdate();		

		}
    }
}