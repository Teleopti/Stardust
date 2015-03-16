using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class GetTeamAdherenceTest
	{
		[Test]
		public void GetOutOfAdherenceForTeamsOnSite_ShouldGetTeamsFromReadModel()
		{
			var teamOutOfAdherenceReadModelReader = MockRepository.GenerateMock<ITeamOutOfAdherenceReadModelReader>();
			var teamId = Guid.NewGuid();
			var target = new GetTeamAdherence(null, null,null, teamOutOfAdherenceReadModelReader);
			var siteId = Guid.NewGuid();
			var teamsOutOfAdherence = new List<TeamOutOfAdherenceReadModel>()
			{
				new TeamOutOfAdherenceReadModel(){ TeamId = teamId ,Count= 3}
			};

			teamOutOfAdherenceReadModelReader.Stub(t => t.Read(siteId)).Return(teamsOutOfAdherence);

			IEnumerable<TeamOutOfAdherence> result = target.GetOutOfAdherenceForTeamsOnSite(siteId.ToString());

			result.Single().OutOfAdherence.Should().Be(3);
			result.Single().Id.Should().Be(teamId.ToString());
		}

	}

}