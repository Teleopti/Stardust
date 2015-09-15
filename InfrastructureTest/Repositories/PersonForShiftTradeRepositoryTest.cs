using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PersonForShiftTradeRepositoryTest : DatabaseTest
	{
		private PersonForScheduleFinder _target;

		[Test]
		public void ShouldLoadPersonInSpecificTeamForShiftTrade()
		{
			var date = new DateOnly(2012, 12, 27);
			var team = TeamFactory.CreateSimpleTeam("team one");
			var person = persistPerson(date, team);

			_target = new PersonForScheduleFinder(UnitOfWork);
			var teamIdList = new List<Guid> {team.Id.GetValueOrDefault()};
			var result = _target.GetPersonFor(date, teamIdList, "anna");

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.Value);
			result.First().TeamId.Should().Be.EqualTo(team.Id.Value);
			result.First().SiteId.Should().Be.EqualTo(team.Site.Id.Value);
			result.First().BusinessUnitId.Should().Be.EqualTo(team.Site.BusinessUnit.Id.Value);
		}

		private IPerson persistPerson(DateOnly startDate, ITeam team)
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var site = SiteFactory.CreateSimpleSite("new site");
			businessUnit.AddSite(site);
			site.AddTeam(team);
			var workflowControlSet = new WorkflowControlSet("new set");

			var person1Period = PersonPeriodFactory.CreatePersonPeriod(startDate, team);
			var person1 = PersonFactory.CreatePerson("anna", "andersson");
			person1.AddPersonPeriod(person1Period);
			person1.WorkflowControlSet = workflowControlSet;
			
			var person2Period = PersonPeriodFactory.CreatePersonPeriod(startDate, team);
			var person2 = PersonFactory.CreatePerson("steven", "selter");
			person2.AddPersonPeriod(person2Period);
			person2.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(businessUnit);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person1Period.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(person1Period.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(person1Period.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(person2Period.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(person2Period.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(person2Period.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(workflowControlSet);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);

			var businessUnitId = ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();

			const string sqlTemplate
				= @"INSERT INTO [ReadModel].[GroupingReadOnly] "
				  + "([PersonId],[FirstName],[LastName],[StartDate],[TeamId],[SiteId],[BusinessUnitId] ,[GroupId],[PageId]) "
				  + "VALUES ('{0}','{1}','{2}','2012-02-02 00:00:00' ,'{3}','{4}','{5}','{6}','6CE00B41-0722-4B36-91DD-0A3B63C545CF')";

			var populateReadModelQuery
				= string.Format(sqlTemplate, person1.Id.Value, person1.Name.FirstName, person1.Name.LastName,
					team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), Guid.Parse(businessUnitId.ToString()),
					team.Id.GetValueOrDefault());

			Session.CreateSQLQuery(populateReadModelQuery).ExecuteUpdate();

			return new PersonRepository(UnitOfWork).Get(person1.Id.Value);
		}
	}
}
