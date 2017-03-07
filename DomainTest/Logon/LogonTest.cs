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

namespace Teleopti.Ccc.DomainTest.Logon
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
		public ClaimSetForApplicationRole ClaimSetForApplicationRole;

		[Test]
		public void ShouldHavePermissionsOfRole()
		{
			var personId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithPerson(personId, "roger")
				.WithRole(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			var person = Persons.Load(personId);
			var dataSource = DataSourceForTenant.Tenant("tenant");
			var businessUnit = BusinessUnits.LoadAll().Single();

			LogOnOff.LogOn(dataSource, person, businessUnit);
			foreach (var role in person.PermissionInformation.ApplicationRoleCollection)
				Principal.Current().AddClaimSet(ClaimSetForApplicationRole.Transform(role, "tenant"));

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview).Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SeatPlanner).Should().Be.False();
		}
	}
}