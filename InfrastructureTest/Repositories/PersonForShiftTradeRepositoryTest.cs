using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PersonForShiftTradeRepositoryTest : DatabaseTest
	{
		private PersonForShiftTradeRepository _target;

		[Test]
		public void ShouldLoadPersonInSpecificTeamForShiftTrade()
		{
			var date = new DateOnly(2012, 12, 27);
			var team = TeamFactory.CreateSimpleTeam("team one");
			var person = persistPerson(date, team);

			_target = new PersonForShiftTradeRepository(UnitOfWork);
			var result = _target.GetPersonForShiftTrade(date, team.Id.Value);

			result.First().PersonId.Should().Be.EqualTo(person.Id.Value);
			result.First().TeamId.Should().Be.EqualTo(team.Id.Value);
			result.First().SiteId.Should().Be.EqualTo(team.Site.Id.Value);
			result.First().BusinessUnitId.Should().Be.EqualTo(team.Site.BusinessUnit.Id.Value);
		}

		[Test]
		public void ShouldLoadPersonInAnyTeamForShiftTrade()
		{
			var date = new DateOnly(2012, 12, 27);
			var team1 = TeamFactory.CreateSimpleTeam("team one");
			var person1 = persistPerson(date, team1);
			var team2 = TeamFactory.CreateSimpleTeam("team two");
			var person2 = persistPerson(date, team2);

			_target = new PersonForShiftTradeRepository(UnitOfWork);
			var result = _target.GetPersonForShiftTrade(date, null);

			var firstPerson = result.First();
			var lastPerson = result.Last();

			firstPerson.PersonId.Should().Be.EqualTo(person1.Id.Value);
			firstPerson.TeamId.Should().Be.EqualTo(team1.Id.Value);
			firstPerson.SiteId.Should().Be.EqualTo(team1.Site.Id.Value);
			firstPerson.BusinessUnitId.Should().Be.EqualTo(team1.Site.BusinessUnit.Id.Value);

			lastPerson.PersonId.Should().Be.EqualTo(person2.Id.Value);
			lastPerson.TeamId.Should().Be.EqualTo(team2.Id.Value);
			lastPerson.SiteId.Should().Be.EqualTo(team2.Site.Id.Value);
			lastPerson.BusinessUnitId.Should().Be.EqualTo(team2.Site.BusinessUnit.Id.Value);
		}

		private IPerson persistPerson(DateOnly startDate, ITeam team)
		{
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var site = SiteFactory.CreateSimpleSite("new site");
			businessUnit.AddSite(site);
			site.AddTeam(team);
			var workflowControlSet = new WorkflowControlSet("new set");
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, team);
			var person = PersonFactory.CreatePerson("anna", "andersson");
			person.AddPersonPeriod(personPeriod);
			person.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(businessUnit);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(workflowControlSet);
			PersistAndRemoveFromUnitOfWork(person);

			return new PersonRepository(UnitOfWork).Get(person.Id.Value);
		}
	}
}
