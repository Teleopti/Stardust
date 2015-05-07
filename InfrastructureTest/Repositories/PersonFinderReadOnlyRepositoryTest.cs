using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class PersonFinderReadOnlyRepositoryTest : DatabaseTest
    {
        private IPersonFinderReadOnlyRepository _target;

		protected override void SetupForRepositoryTest()
	    {
			createAndSaveReadModel();
			UnitOfWork.PersistAll();
			CleanUpAfterTest();
	    }

        [Test]
        public void ShouldLoadPersonsWithOneCriteria()
        {
            var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "agent", 10,
                                                             new DateOnly(2012, 1, 1), 1, 1);
            _target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.Find(crit);
            Assert.That(crit.TotalRows, Is.EqualTo(2));
        }

		[Test]
		public void ShouldLoadPersonsWithMultipleCriteria()
		{
			var criterias = new Dictionary<PersonFinderField, string>();
			criterias.Add(PersonFinderField.FirstName,"ashley");
			criterias.Add(PersonFinderField.Role,"agent");
			var crit = new PersonFinderSearchCriteria(criterias, 10,
															 new DateOnly(2012, 1, 1), 1, 1);
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
																			 new DateOnly(1012, 1, 1), 1, 1);
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));
		}

	    private void createAndSaveReadModel()
	    {
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Pierre','FirstName');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Baldi','LastName');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Team Preferences London','Organization');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Agent','Role');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('B0E35119-4661-4A1B-8772-9B5E015B2564','Pierre','Baldi','137567','',NULL,'Email','Skill');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'137545','EmploymentNumber');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Ashley','FirstName');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Andeen','LastName');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Team Preferences London','Organization');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Agent','Role');")
			    .ExecuteUpdate();
		    Session.CreateSQLQuery(
			    "Insert into [ReadModel].[FindPerson](PersonId,FirstName,LastName,EmploymentNumber,Note,TerminalDate,SearchValue,SearchType)" +
			    " Values ('11610FE4-0130-4568-97DE-9B5E015B2564','Ashley','Andeen','137545','',NULL,'Direct Sales','Skill');")
			    .ExecuteUpdate();

	    }
    }
}