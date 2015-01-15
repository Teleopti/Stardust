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
	public class GetSiteAdherenceTest
	{
		[Test]
		public void ShouldReadAdherenceForAllSitesFromReadModel()
		{
			var buId = Guid.NewGuid();
			var site = new Site("s").WithId();
			var siteAdherencePersister = MockRepository.GenerateMock<ISiteOutOfAdherenceReadModelPersister>();
			siteAdherencePersister.Stub(x => x.GetForBusinessUnit(buId))
				.Return(new List<SiteOutOfAdherenceReadModel>()
			          {
				          new SiteOutOfAdherenceReadModel()
				          {
					          SiteId = site.Id.GetValueOrDefault(),
					          Count = 1
				          }
			          });

			var target = new GetSiteAdherence(siteAdherencePersister);

			var result = target.ReadAdherenceForAllSites(buId);

			result.Single().Id.Should().Be(site.Id.ToString());
			result.Single().OutOfAdherence.Should().Be(1);
		}

	}
}
