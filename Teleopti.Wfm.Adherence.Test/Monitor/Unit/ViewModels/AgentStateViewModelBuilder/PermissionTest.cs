using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	[FakePermissions]
	public class PermissionTest : IIsolateSystem
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePermissions Permissions;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
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
			Permissions.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, siteId1);
			var agentState = Target.Build(new AgentStateFilter { SiteIds = new[] { siteId1, siteId2 } }).States.ToArray();

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
			Permissions.HasPermissionToTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, teamId);
			var agentState = Target.Build(new AgentStateFilter { SiteIds = new[] { siteId } }).States.ToArray();

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
			Permissions.HasPermissionToTeam(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, teamId);
			var agentState = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States.ToArray();

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1);
		}

	}
}