using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels
{
	[DomainTest]
	[TestFixture]
	[DefaultData]
	[FakePermissions]
	public class OrganizationViewModelBuilderPermissionTest
	{
		public OrganizationViewModelBuilder Target;
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
			
			result.Select(x => x.Id)
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

			result.Select(x => x.Id)
				.Should().Have.SameValuesAs(site);
		}


		[Test]
		public void ShouldOnlyIncludePermittedTeam()
		{
			var site = Guid.NewGuid();
			var team = Guid.NewGuid();
			var wrongTeam = Guid.NewGuid();
			var person = Guid.NewGuid();
			var wrongPerson = Guid.NewGuid();
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
				.WithTeam(wrongTeam)
				.WithAgent(wrongPerson)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = wrongPerson,
					BusinessUnitId = Database.CurrentBusinessUnitId(),
					SiteId = site,
					TeamId = wrongTeam
				})
				;
			Permissions.HasPermissionToTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, team);

			var result = Target.Build();

			result.SelectMany(x => x.Teams.Select(y => y.Id))
				.Should().Have.SameValuesAs(team);
		}
	}
}