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

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class PermissionTest
	{
		public FakeDatabase Database;
		public FakeBusinessUnitRepository BusinessUnits;
		public FakePersonRepository Persons;
		public FakeApplicationFunctionRepository ApplicationFunctions;

		public ILogOnOff LogOnOff;
		public IAuthorization Authorization;
		public ICurrentTeleoptiPrincipal Principal;
		public ClaimSetForApplicationRole ClaimSetForApplicationRole;

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

			LogOnOff.ProperLogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), myTeam)
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), otherTeam)
				.Should().Be.False();
		}
		
		[Test]
		public void ShouldHavePermissionForMySite()
		{
			var meId = Guid.NewGuid();
			var otherGuyId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithSite("other site")
				.WithTeam("other team")
				.WithAgent(otherGuyId, "other guy")

				.WithSite("my site")
				.WithTeam("my team")
				.WithAgent(meId, "me")
				.WithRole(AvailableDataRangeOption.MySite, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var me = Persons.Load(meId);
			var mySite = me.MyTeam("2017-03-07".Date()).Site;
			var otherSite = Persons.Load(otherGuyId).MyTeam("2017-03-07".Date()).Site;

			LogOnOff.ProperLogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), mySite)
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), otherSite)
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

			LogOnOff.ProperLogOn("tenant", me, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), myTeam)
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), otherTeam)
				.Should().Be.True();
		}
	}
}