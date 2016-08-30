using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[ReadModelUpdaterTest]
	[Toggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
	public class PersonChangedTeamTest
	{
		public AgentStateReadModelUpdaterWithAssociationUpdate Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldMovePersonToNewTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = teamId
			});

			Persister.Models.Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldMovePersonToNewTeamOnDifferentSite()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				SiteId = siteId
			});

			Persister.Models.Single().SiteId.Should().Be(siteId);
		}
	}
}