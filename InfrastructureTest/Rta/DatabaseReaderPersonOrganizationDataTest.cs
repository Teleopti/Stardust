using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class DatabaseReaderPersonOrganizationDataTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldFetchTeamForPerson()
		{
			var team = TeamFactory.CreateTeam("t", "s");
			var person = new Person();
			person.AddPersonPeriod(createPersonPeriodAndPersistDependencies(team));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);

			UnitOfWork.PersistAll();

			var teamId = team.Id.Value;
			var personId = person.Id.Value;

			var target = new DatabaseReader(new FakeConnectionStrings(), new Now());
			var resItem = target.PersonOrganizationData().Single(x => x.PersonId == personId);
			resItem.TeamId.Should().Be.EqualTo(teamId);
			resItem.PersonId.Should().Be.EqualTo(personId);

			//cleanup -remove later
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.Delete(person);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.ContractSchedule);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.Contract);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.PartTimePercentage);
				session.Delete(team);
				session.Delete(team.Site);
				uow.PersistAll();
			}
		}


		[Test]
		public void ShouldFetchSiteForPerson()
		{
			var team = TeamFactory.CreateTeam("t", "s");
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

			var target = new DatabaseReader(new FakeConnectionStrings(), new Now());
			var resItem = target.PersonOrganizationData().Single(x => x.PersonId == personId);
			resItem.SiteId.Should().Be.EqualTo(siteId);
			resItem.PersonId.Should().Be.EqualTo(personId);

			//cleanup -remove later
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.Delete(person);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.ContractSchedule);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.Contract);
				session.Delete(person.PersonPeriodCollection.Single().PersonContract.PartTimePercentage);
				session.Delete(team);
				session.Delete(site);
				uow.PersistAll();
			}
		}


		private PersonPeriod createPersonPeriodAndPersistDependencies(ITeam team)
		{
			var ptp = new PartTimePercentage("ptp");
			var contract = new Contract("c");
			var contractSchedule = new ContractSchedule("cs");
			var pp = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(contract, ptp, contractSchedule), team);

			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(ptp);
			return pp;
		}
	}
}