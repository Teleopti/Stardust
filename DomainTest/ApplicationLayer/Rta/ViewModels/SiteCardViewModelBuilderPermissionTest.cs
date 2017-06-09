using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	[FakePermissions]
	public class SiteCardViewModelBuilderPermissionTest
	{
		public SiteCardViewModelBuilder Target;
		public FakePermissions Permissions;
		public FakeDatabase Database;
		
		[Test]
		public void ShouldExcludeNonPermittedSites()
		{
			var site1 = Guid.NewGuid();
			var team1 = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var site2 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithSite(site1)
				.WithTeam(team1)
				.WithAgent(person1)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = person1,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = site1,
					TeamId = team1
				})
				.WithSite(site2)
				.WithTeam(team2)
				.WithAgent(person2)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = person2,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = site2,
					TeamId = team2
				})
				;
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, new SiteAuthorization {BusinessUnitId = Database.CurrentBusinessUnitId(), SiteId = site1});

			var result = Target.Build();

			result.Select(x => x.Id)
				.Should().Have.SameValuesAs(site1);
		}

		//[Test, Ignore("WIP")]
		//public void ShouldBuildForPermittedSiteOnlyForTeamLevel()
		//{
		//	var meId = Guid.NewGuid();
		//	var siteId = Guid.NewGuid();
	
		//	Database
		//		.WithTenant("tenant")
		//		.WithSite(siteId)

		//		.WithTeam("my team")
		//		.WithAgent(meId, "me")
		//		.WithRole(AvailableDataRangeOption.MyTeam, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)
		//		;
		//	var me = Persons.Load(meId);
			
		//	LogOnOff.LogOn("tenant", me, Database.CurrentBusinessUnitId());


		//	var viewModel = Target.Build().Single();

		//	viewModel.Id.Should().Be(siteId);


		//}

	}
}