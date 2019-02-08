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
	public class AvailableTeamTest
	{
		public FakeDatabase Database;
		public FakePersonRepository Persons;

		public ILogOnOff LogOnOff;
		public IAuthorization Authorization;

		[Test]
		public void ShouldHavePermissionForMyTeam()
		{
			var meId = Guid.NewGuid();
			var otherGuyId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithTeam("other team")
				.WithAgent(otherGuyId, "other guy")

				.WithTeam("my team")
				.WithAgent(meId, "me")
				.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			var myTeam = me.MyTeam("2017-03-07".Date());
			var otherTeam = Persons.Load(otherGuyId).MyTeam("2017-03-07".Date());

			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				myTeam)
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				otherTeam)
				.Should().Be.False();
		}

		[Test]
		public void ShouldHavePermissionForOtherTeamTroughSitePermission()
		{
			var meId = Guid.NewGuid();
			var otherGuyId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithSite("our site")
				.WithTeam("other team")
				.WithAgent(otherGuyId, "other guy")

				.WithTeam("my team")
				.WithAgent(meId, "me")
				.WithRole(AvailableDataRangeOption.MySite, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			var myTeam = me.MyTeam("2017-03-07".Date());
			var otherTeam = Persons.Load(otherGuyId).MyTeam("2017-03-07".Date());

			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				myTeam)
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				otherTeam)
				.Should().Be.True();
		}

		[Test]
		public void ShouldHavePermissionsForTeamTroughAvailableData()
		{
			var meId = Guid.NewGuid();
			var otherGuyId = Guid.NewGuid();
			var otherTeamId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithTeam(otherTeamId, "other team")
				.WithAgent(otherGuyId, "other guy")
				.WithPerson(meId, "me")
				.WithRole(AvailableDataRangeOption.None, otherTeamId, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			var otherTeam = Persons.Load(otherGuyId).MyTeam("2017-03-07".Date());

			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				otherTeam)
				.Should().Be.True();
		}
	}
}