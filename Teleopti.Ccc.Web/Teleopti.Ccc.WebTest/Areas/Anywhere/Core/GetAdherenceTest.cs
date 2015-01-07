using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using System.Collections.Generic;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class GetAdherenceTest
	{
		[Test]
		public void ShouldReadAdherenceForAllSitesFromReadModel()
		{
			var buId = Guid.NewGuid();
			var site = new Site("s").WithId();
			var siteAdherencePersister = MockRepository.GenerateMock<ISiteAdherencePersister>();
			siteAdherencePersister.Stub(x => x.GetAll(buId))
				.Return(new List<SiteAdherenceReadModel>()
			          {
				          new SiteAdherenceReadModel()
				          {
					          SiteId = site.Id.GetValueOrDefault(),
					          AgentsOutOfAdherence = 1
				          }
			          });

			var target = new GetAdherence(null, null, null, null, null, siteAdherencePersister);

			var result = target.ReadAdherenceForAllSites(buId);

			result.Single().Id.Should().Be(site.Id.ToString());
			result.Single().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void GetOutOfAdherenceForTeamsOnSite_ShouldGetTeamsFromReadModel()
		{
			var teamAdherencePersister = MockRepository.GenerateMock<ITeamAdherencePersister>();
			var teamId = Guid.NewGuid();
			var target = new GetAdherence(null, null, null, null, teamAdherencePersister, null);
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
	}
}
