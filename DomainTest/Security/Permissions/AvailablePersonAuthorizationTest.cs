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
	[DefaultData]
	[RealPermissions]
	[AddDatasourceId]
	public class AvailablePersonAuthorizationTest
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
			var myTeam = me.MyTeam("2017-03-07".Date());
			var siteId = myTeam.Site.Id.Value;
			var otherGuyDetails = new PersonAuthorization
			{
				PersonId = otherGuyId,
				BusinessUnitId = Database.CurrentBusinessUnitId(),
				SiteId = siteId,
				TeamId = otherTeamId
			};
			var myDetails = new PersonAuthorization
			{
				PersonId = meId,
				BusinessUnitId = Database.CurrentBusinessUnitId(),
				SiteId = siteId,
				TeamId = myTeamId
			};

			LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());
			
			Authorization.IsPermitted(
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), myDetails)
				.Should().Be.True();
			Authorization.IsPermitted(
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "2017-03-07".Date(), otherGuyDetails)
				.Should().Be.False();
		}

	}
}