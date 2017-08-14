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

namespace Teleopti.Ccc.DomainTest.Logon.Permissions
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class AvailableTeamAuthorizationTest
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
					new TeamAuthorization
					{
						BusinessUnitId = myTeam.Site.BusinessUnit.Id.Value,
						SiteId = myTeam.Site.Id.Value,
						TeamId = myTeam.Id.Value
					})
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
					new TeamAuthorization
					{
						BusinessUnitId = otherTeam.Site.BusinessUnit.Id.Value,
						SiteId = otherTeam.Site.Id.Value,
						TeamId = otherTeam.Id.Value
					})
				.Should().Be.False();
		}
		
	}
}