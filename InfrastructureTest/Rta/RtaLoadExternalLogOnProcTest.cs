using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	public class RtaLoadExternalLogOnProcTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldOnlyReturnActivePersonPeriodExternalLogOn()
		{
			const string query = "EXEC [dbo].[rta_load_external_logon] :date";
			var person = new Person();
			var team = TeamFactory.CreateTeam("_", "_");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			var site = team.Site;
			var contract = new Contract("_");
			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-14),
				new PersonContract(contract, partTimePercentage, contractSchedule), team);
			var personPeriod2 = new PersonPeriod(DateOnly.Today,
				new PersonContract(contract, partTimePercentage, contractSchedule), team);
			var externalLogOn = new ExternalLogOn(1, 1, "_", "_", true);
			personPeriod.AddExternalLogOn(externalLogOn);
			personPeriod2.AddExternalLogOn(externalLogOn);
			person.AddPersonPeriod(personPeriod);
			person.AddPersonPeriod(personPeriod2);
			PersistAndRemoveFromUnitOfWork(externalLogOn);
			PersistAndRemoveFromUnitOfWork(contractSchedule);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(partTimePercentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(person);
			UnitOfWork.PersistAll();

			Session.CreateSQLQuery(query)
				.SetDateTime("date",DateTime.Today)
				.List().Count
				.Should().Be.EqualTo(1);

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.Delete(person);
				session.Delete(team);
				session.Delete(site);
				session.Delete(partTimePercentage);
				session.Delete(contractSchedule);
				session.Delete(contract);
				session.Delete(externalLogOn);
				uow.PersistAll();
			}
		}
	}
}