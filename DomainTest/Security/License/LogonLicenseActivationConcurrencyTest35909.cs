using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Security.License
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class LogonLicenseActivationConcurrencyTest35909
	{
		public FakeDatabase Database;
		public FakeBusinessUnitRepository BusinessUnits;
		public FakePersonRepository Persons;
		public FakeApplicationFunctionRepository ApplicationFunctions;

		public IDataSourceForTenant DataSourceForTenant;
		public ILogOnOff LogOnOff;
		public IAuthorization Authorization;
		public ICurrentTeleoptiPrincipal Principal;
		public ConcurrencyRunner Run;
		
		[Test]
		public void ShouldHandleConcurrency()
		{
			var personId = Guid.NewGuid();
			100.Times(i => Database.WithTenant($"tenant{i}"));
			Database
				.WithPerson(personId, "roger")
				.WithRole(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);

			Run.InParallel(() =>
			{
				100.Times(i =>
				{
					var person = Persons.Load(personId);
					var dataSource = DataSourceForTenant.Tenant($"tenant{i}");
					var businessUnit = BusinessUnits.LoadAll().Single();
					LogOnOff.LogOn(dataSource, person, businessUnit);
					
					Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview).Should().Be.True();
					Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SeatPlanner).Should().Be.False();
				});

			})
			.Times(10)
			.Wait();
		}
	}
}