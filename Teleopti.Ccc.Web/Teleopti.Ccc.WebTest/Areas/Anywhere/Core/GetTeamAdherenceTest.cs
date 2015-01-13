using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class GetTeamAdherenceTest
	{
		[Test]
		public void GetOutOfAdherenceForTeamsOnSite_ShouldGetTeamsFromReadModel()
		{
			var teamAdherencePersister = MockRepository.GenerateMock<ITeamAdherencePersister>();
			var teamId = Guid.NewGuid();
			var target = new GetTeamAdherence(null, null,null, teamAdherencePersister);
			var siteId = Guid.NewGuid();
			var teamsOutOfAdherence = new List<TeamAdherenceReadModel>()
			{
				new TeamAdherenceReadModel(){ TeamId = teamId ,AgentsOutOfAdherence= 3}
			};

			teamAdherencePersister.Stub(t => t.GetForSite(siteId)).Return(teamsOutOfAdherence);

			IEnumerable<TeamOutOfAdherence> result = target.GetOutOfAdherenceForTeamsOnSite(siteId.ToString());

			result.Single().OutOfAdherence.Should().Be(3);
			result.Single().Id.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldGetTeamAdherenceForSingleTeam()
		{
			var teamAdherencePersister = MockRepository.GenerateMock<ITeamAdherencePersister>();
			var teamId = Guid.NewGuid();
			var target = new GetTeamAdherence(null, null,null, teamAdherencePersister);
			var teamsOutOfAdherence = new TeamAdherenceReadModel { TeamId = teamId, AgentsOutOfAdherence = 3 };
			teamAdherencePersister.Stub(t => t.Get(teamId)).Return(teamsOutOfAdherence);
			var result = target.GetOutOfAdherenceLite(teamId.ToString());
			result.Id.Should().Be(teamId.ToString());
			result.OutOfAdherence.Should().Be(3);
		}

		[Test]
		public void ShouldHaveZeroAdherenceIfReadModelDoesNotExists()
		{
			var teamAdherencePersister = MockRepository.GenerateMock<ITeamAdherencePersister>();
			var teamId = Guid.NewGuid();
			var target = new GetTeamAdherence(null, null,null, teamAdherencePersister);
			teamAdherencePersister.Stub(t => t.Get(teamId)).Return(null);
			var result = target.GetOutOfAdherenceLite(teamId.ToString());
			result.Id.Should().Be(teamId.ToString());
			result.OutOfAdherence.Should().Be(0);
		}
		
	}

}