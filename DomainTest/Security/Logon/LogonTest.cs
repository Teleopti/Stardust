using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Security.Logon
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class LogonTest
	{
		public FakeDatabase Database;
		public FakeBusinessUnitRepository BusinessUnits;
		public FakePersonRepository Persons;
		public FakeApplicationFunctionRepository ApplicationFunctions;

		public IDataSourceForTenant DataSourceForTenant;
		public ILogOnOff LogOnOff;
		public IAuthorization Authorization;
		public ICurrentTeleoptiPrincipal Principal;

		[Test]
		public void ShouldHavePermissionsOfRole()
		{
			var personId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithPerson(personId, "roger")
				.WithRole(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			var person = Persons.Load(personId);

			LogOnOff.LogOn("tenant", person, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview).Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SeatPlanner).Should().Be.False();
		}
	}
}