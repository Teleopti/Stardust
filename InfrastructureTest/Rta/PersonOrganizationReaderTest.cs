using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class PersonOrganizationReaderTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldFetchTeamForPerson()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			var person = new Person();
			person.AddPersonPeriod(createPersonPeriodAndPersistDependencies(team));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);

			UnitOfWork.PersistAll();

			var teamId = team.Id.Value;
			var personId = person.Id.Value;

			var target = new PersonOrganizationReader(new Now(), Session.Connection.ConnectionString);
			var resItem = target.LoadAll().Single(x => x.PersonId == personId);
			resItem.TeamId.Should().Be.EqualTo(teamId);
			resItem.PersonId.Should().Be.EqualTo(personId);

			//cleanup -remove later
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new Repository(uow);
				rep.Remove(team);
				rep.Remove(team.Site);
				rep.Remove(person);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.ContractSchedule);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.Contract);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.PartTimePercentage);
				uow.PersistAll();
			}
		}


		[Test]
		public void ShouldFetchSiteForPerson()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			var site = team.Site;
			var person = new Person();
			person.AddPersonPeriod(createPersonPeriodAndPersistDependencies(team));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);

			UnitOfWork.PersistAll();

			var siteId = site.Id.Value;
			var personId = person.Id.Value;
	
			var target = new PersonOrganizationReader(new Now(), Session.Connection.ConnectionString);
			var resItem = target.LoadAll().Single(x => x.PersonId == personId);
			resItem.SiteId.Should().Be.EqualTo(siteId);
			resItem.PersonId.Should().Be.EqualTo(personId);

			//cleanup -remove later
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new Repository(uow);
				rep.Remove(team);
				rep.Remove(site);
				rep.Remove(person);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.ContractSchedule);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.Contract);
				rep.Remove(person.PersonPeriodCollection.Single().PersonContract.PartTimePercentage);
				uow.PersistAll();
			}
		}


		private PersonPeriod createPersonPeriodAndPersistDependencies(ITeam team)
		{
			var ptp = new PartTimePercentage(" ");
			var contract = new Contract(" ");
			var contractSchedule = new ContractSchedule(" ");
			var pp = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(contract, ptp, contractSchedule), team);

			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(ptp);
			return pp;
		}
	}
}