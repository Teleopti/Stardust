using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class GetSiteAdherenceTest
	{
		[Test]
		public void ShouldReadAdherenceForAllPermittedSitesFromReadModel()
		{
			var site = new Site("s").WithId();
			var permittedSites = new[] {site};
			var now = new Now();
			var siteAdherencePersister = MockRepository.GenerateMock<ISiteOutOfAdherenceReadModelReader>();
			var availableSitesProvider = MockRepository.GenerateMock<IPersonalAvailableDataProvider>();
			siteAdherencePersister.Stub(x => x.Read(new[]{site.Id.Value}))
				.Return(new List<SiteOutOfAdherenceReadModel>()
			          {
				          new SiteOutOfAdherenceReadModel()
				          {
					          SiteId = site.Id.GetValueOrDefault(),
					          Count = 1
				          }
			          });
			availableSitesProvider.Stub(x => x.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, now.LocalDateOnly()))
				.Return(permittedSites);

			var target = new GetSiteAdherence(siteAdherencePersister, availableSitesProvider, now);

			var result = target.ReadAdherenceForAllPermittedSites();

			result.Single().Id.Should().Be(site.Id.ToString());
			result.Single().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldReturnEmtpyIfNotSiteAvailable()
		{
			var now = new Now();
			var availableSitesProvider = MockRepository.GenerateMock<IPersonalAvailableDataProvider>();
			availableSitesProvider.Stub(x => x.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, now.LocalDateOnly()))
				.Return(new List<ISite>());

			var target = new GetSiteAdherence(null, availableSitesProvider, now);

			var result = target.ReadAdherenceForAllPermittedSites();

			result.Should().Be.Empty();
		}
	}
}
