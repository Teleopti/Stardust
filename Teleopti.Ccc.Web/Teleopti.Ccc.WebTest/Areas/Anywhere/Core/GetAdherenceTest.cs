using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class GetAdherenceTest
	{
		[Test]
		public void ShouldReadAdherenceForAllSitesFromReadModel()
		{
			var site = new Site("s").WithId();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var siteAdherencePersister = MockRepository.GenerateMock<ISiteAdherencePersister>();
			siteRepository.Stub(x => x.LoadAll()).Return(new List<ISite> {site});
			siteAdherencePersister.Stub(x => x.Get(site.Id.GetValueOrDefault()))
				.Return(new SiteAdherenceReadModel {SiteId = site.Id.GetValueOrDefault(), AgentsOutOfAdherence = 1});

			var target = new GetAdherence(siteRepository, null, null, null, null, siteAdherencePersister);

			var result = target.ReadAdherenceForAllSites();

			result.Single().Id.Should().Be(site.Id.ToString());
			result.Single().OutOfAdherence.Should().Be(1);
		}
	}
}
