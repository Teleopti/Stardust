using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class NumberOfAgentsInSiteReaderTest : DatabaseTest
	{
		[Test]
		public void ShouldLoadNumberOfAgentesForSite()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			var personPeriod = createPersonPeriodAndPersistDependencies(team);

			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			var person2 = new Person();
			person2.AddPersonPeriod(personPeriod.EntityClone());
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);

			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);
			PersistAndRemoveFromUnitOfWork(person2);
			
			var target = new NumberOfAgentsInSiteReader(new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal())), new Now());
			var result = target.FetchNumberOfAgents(new[] {team.Site});

			result[team.Site.Id.Value].Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnSiteWithNoAgents()
		{
			var team = TeamFactory.CreateTeam(" ", " ");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var target = new NumberOfAgentsInSiteReader(new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal())), new Now());
			var result = target.FetchNumberOfAgents(new[] { team.Site });

			result[team.Site.Id.Value].Should().Be.EqualTo(0);
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