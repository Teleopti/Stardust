using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[ReadModelUpdaterTest]
	public class PersonChangedTeamTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldMovePersonToNewTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

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
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				SiteId = siteId
			});

			Persister.Models.Single().SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldMovePersonTonewTeamOnDifferentBusinessUnit()
		{
			var personId = Guid.NewGuid();
			var businessUnit = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
				BusinessUnitId = businessUnit
			});

			Persister.Models.Single().BusinessUnitId.Should().Be(businessUnit);

		}
	}
}