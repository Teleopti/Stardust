using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels
{
	[DomainTest]
	[TestFixture]
	[DefaultData]
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
			Permissions.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, site1);

			var result = Target.Build();

			result.Sites.Select(x => x.Id)
				.Should().Have.SameValuesAs(site1);
		}

		[Test]
		public void ShouldIncludeSiteWithPermittedTeam()
		{
			var site = Guid.NewGuid();
			var team = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam(team)
				.WithAgent(person)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = person,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = site,
					TeamId = team
				})
				;
			Permissions.HasPermissionToTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, team);

			var result = Target.Build();

			result.Sites.Select(x => x.Id)
				.Should().Have.SameValuesAs(site);
		}


	}
}