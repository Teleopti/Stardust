using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Security.Permissions
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	[AddDatasourceId]
	public class AvailablePersonTest
	{
		public FakeDatabase Database;
		public FakePersonRepository Persons;

		public ILogOnOff LogOnOff;
		public IAuthorization Authorization;

		[Test]
		public void ShouldHavePermissionForMyTeam()
		{
			var meId = Guid.NewGuid();
			var myTeamId = Guid.NewGuid();
			var otherGuyId = Guid.NewGuid();
			var otherTeamId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithTeam(otherTeamId, "other team")
				.WithAgent(otherGuyId, "other guy")

				.WithTeam(myTeamId, "my team")
				.WithAgent(meId, "me")
				.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			var otherGuy = Persons.Load(otherGuyId);

			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), me)
				.Should().Be.True();
			Authorization.IsPermitted(
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), otherGuy)
				.Should().Be.False();
		}

	}
}