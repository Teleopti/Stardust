using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[DomainTest]
	public class TeamNameChangedTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldUpdateTeamName()
		{
			var teamId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = Guid.NewGuid(), TeamId = teamId});
			
			Target.Handle(new TeamNameChangedEvent
			{
				TeamId = teamId,
				Name = "team preferences"
			});
			
			Persister.Models.Single().TeamName.Should().Be("team preferences");
		}
	}
}