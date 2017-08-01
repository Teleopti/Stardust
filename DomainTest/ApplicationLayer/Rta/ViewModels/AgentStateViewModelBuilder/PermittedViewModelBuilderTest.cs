using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	[FakePermissions]
	public class PermittedViewModelBuilderTest : ISetup
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePermissions Permissions;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldGetAgentStatesForPermittedSite()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId2
				});
			Permissions.HasPermissionForSite(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, siteId1);
			var agentState = Target.For(new AgentStateFilter { SiteIds = new[] { siteId1, siteId2 } }).States.ToArray();

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldGetAgentStatesForTeamInPermittedSite()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId,
					TeamId = teamId
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId,
					TeamId = Guid.NewGuid()
				});
			Permissions.HasPermissionForTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, teamId);
			var agentState = Target.For(new AgentStateFilter { SiteIds = new[] { siteId } }).States.ToArray();

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1);
		}


		[Test]
		public void ShouldGetAgentStatesForPermittedTeam()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId,
					TeamId = teamId
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId,
					TeamId = Guid.NewGuid()
				});
			Permissions.HasPermissionForTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, teamId);
			var agentState = Target.For(new AgentStateFilter { TeamIds = new[] { teamId } }).States.ToArray();

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1);
		}

	}
}