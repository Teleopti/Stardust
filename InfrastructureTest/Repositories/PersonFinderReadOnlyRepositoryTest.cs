using NUnit.Framework;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
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
		public void ShouldMatchAllValuesInGivenTeamsWithEmptyCriteria()
		{
			var crit = new PersonFinderSearchCriteria(new Dictionary<PersonFinderField, string>(), 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.FindInTeams(crit, new []{team1Id});
			Assert.That(crit.TotalRows, Is.EqualTo(1));
		}

		[Test]
		public void ShouldMatchAllValuesInAllCriteriaInGivenTeams()
		{
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "Ashley Agent", 10,
				new DateOnly(2012, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.FindInTeams(crit, new []{team1Id});
			Assert.That(crit.TotalRows, Is.EqualTo(1));
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
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId, BusinessUnitId, StartDateTime, EndDateTime)" +
				 " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Pierre', 'B0E35119-4661-4A1B-8772-9B5E015B2564','FirstName',:teamId, :siteId, :businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId",buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Baldi','B0E35119-4661-4A1B-8772-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Team Preferences London','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetGuid("businessUnitId", buid)
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Agent','B0E35119-4661-4A1B-8772-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Email','B0E35119-4661-4A1B-8772-9B5E015B2564','Skill', :teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team1Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'137545','11610FE4-0130-4568-97DE-9B5E015B2564','EmploymentNumber',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Ashley','11610FE4-0130-4568-97DE-9B5E015B2564','FirstName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Andeen','11610FE4-0130-4568-97DE-9B5E015B2564','LastName',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Team Preferences London','11610FE4-0130-4568-97DE-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Agent','11610FE4-0130-4568-97DE-9B5E015B2564','Role',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
		    Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E05412741','Ashley Pierre','Andeen','137545','',NULL,'Direct Sales','11610FE4-0130-4568-97DE-9B5E05412741','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2011, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2045, 1, 1))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();
			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'OldSkill','11610FE4-0130-4568-97DE-9B5E015B2564','Skill',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetDateTime("startDateTime", new DateTime(2010, 1, 1))
				.SetDateTime("endDateTime", new DateTime(2010, 12, 31))
				.SetGuid("businessUnitId", buid)
				.SetGuid("teamId", team2Id)
				.SetGuid("siteId", siteId)
				.ExecuteUpdate();

			Session.CreateSQLQuery(
				"Insert into [ReadModel].[FindPerson] (PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId,SearchType, TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
				" Values ('C0E35119-4661-4A1B-8772-9B5E015B2564','Petter','Bay','137567','',NULL,'X''mas Team','B0E35119-4661-4A1B-8772-9B5E015B2564','Organization',:teamId, :siteId,:businessUnitId, :startDateTime, :endDateTime)")
				.SetGuid("businessUnitId",buid)
				.SetDateTime("startDateTime",new DateTime(2011,1,1))
				.SetDateTime("endDateTime",new DateTime(2045,1,1))
				.SetGuid("teamId",Guid.NewGuid())
				.SetGuid("siteId",siteId)
				.ExecuteUpdate();

		}
    }
}