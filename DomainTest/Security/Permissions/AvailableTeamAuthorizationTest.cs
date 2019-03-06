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
						BusinessUnitId = myTeam.Site.GetOrFillWithBusinessUnit_DONTUSE().Id.Value,
						SiteId = myTeam.Site.Id.Value,
						TeamId = myTeam.Id.Value
					})
				.Should().Be.True();
			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
					new TeamAuthorization
					{
						BusinessUnitId = otherTeam.Site.GetOrFillWithBusinessUnit_DONTUSE().Id.Value,
						SiteId = otherTeam.Site.Id.Value,
						TeamId = otherTeam.Id.Value
					})
				.Should().Be.False();
		}

		[Test]
		public void ShouldNotHaveSiteAuthorizationForMyTeamPermission()
		{
			var businessUnit = Guid.NewGuid();
			var site = Guid.NewGuid();
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			var personOnTeam1 = Guid.NewGuid();
			var personOnTeam2 = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnit)
				.WithSite(site)
				.WithTeam(team1, "team 1")
				.WithAgent(personOnTeam1, "blip")
				.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				.WithTeam(team2, "team 2")
				.WithAgent(personOnTeam2, "blop")
				.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
				;
			var person1 = Persons.Load(personOnTeam1);

			LogOnOff.LogOn("tenant", person1, Database.CurrentBusinessUnitId());

			Authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(),
				new SiteAuthorization {BusinessUnitId = businessUnit, SiteId = site}
			).Should().Be.False();
		}
	}
}