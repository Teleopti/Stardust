using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


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

			// Handle businessunit change.
			//((ITeleoptiIdentity) ClaimsPrincipal.Current.Identity).ChangeBusinessUnit(businessUnit);
			Session.EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", businessUnit.Id.GetValueOrDefault());

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

			const string populateReadModelQuery
				= @"INSERT INTO [ReadModel].[GroupingReadOnly] ([PersonId],[FirstName],[LastName],[StartDate],[TeamId],"
				+ "[SiteId],[BusinessUnitId],[GroupId],[PageId]) VALUES (:personId,:firstName,:lastName,:startDate,:teamId,:siteId,:businessUnitId,:groupId,:pageId)";
			
			Session.CreateSQLQuery(populateReadModelQuery)
				.SetGuid("personId",person1.Id.GetValueOrDefault())
				.SetString("firstName",person1.Name.FirstName)
				.SetString("lastName",person1.Name.LastName)
				.SetDateTime("startDate", new DateTime(2012,02,02))
				.SetGuid("teamId",team.Id.GetValueOrDefault())
				.SetGuid("siteId",site.Id.GetValueOrDefault())
				.SetGuid("businessUnitId", businessUnit.Id.GetValueOrDefault())
				.SetGuid("groupId", team.Id.GetValueOrDefault())
				.SetGuid("pageId", Guid.NewGuid())
				.ExecuteUpdate();

			return PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(UnitOfWork), null, null).Get(person1.Id.GetValueOrDefault());
		}
	}
}
