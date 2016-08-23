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
	public class TerminatedPersonTest
	{
		public AgentStateReadModelCleaner Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldRemoveReadModelIfPersonIsTerminated()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Models.Should().Be.Empty();
		}

		[Test]
		public void ShouldKeepReadModelIfPersonIsInATeam()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.Empty
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
		}
		[Test]
		[Toggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
		public void ShouldMovePersonToNewTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = teamId
			});

			Persister.Models.Single().TeamId.Should().Be(teamId);
		}

		[Test]
		[Toggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
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