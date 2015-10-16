using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class SitesControllerTest
	{
		[Test]
		public void ShouldGetSitesForMyBusinessUnit()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInSiteReader>();
			var target = new SitesController(siteRepository, numberOfAgentsQuery,null, null, null,null);
			var site = new Site("London");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.LoadAll()).Return(new[] { site });
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { site })).Return(new Dictionary<Guid, int>() { { site.Id.Value, 0 } });
			
			var result = target.Index().Data as IEnumerable<SiteViewModel>;

			result.Single().Id.Should().Be(site.Id.Value.ToString());
			result.Single().Name.Should().Be(site.Description.Name);
		}

		[Test]
		public void ShouldGetOnlyPermittedSitesForMyBusinessUnit()
		{
			var now = new Now();
			var site1 = new Site("Permitted");
			site1.SetId(Guid.NewGuid());
			var site2 = new Site("NotPermitted");
			site2.SetId(Guid.NewGuid());
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInSiteReader>();
			var personalAvailableDataProvider = MockRepository.GenerateMock<IPersonalAvailableDataProvider>();
			siteRepository.Stub(x => x.LoadAll()).Return(new[] { site1, site2 });
			personalAvailableDataProvider.Stub(
				x => x.AvailableSites(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, now.LocalDateOnly()))
				.Return(new[] {site1});
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { site1 })).Return(new Dictionary<Guid, int>() { { site1.Id.Value, 0 } });
			
			var target = new SitesController(siteRepository, numberOfAgentsQuery, null, null, personalAvailableDataProvider, now);
			var result = target.Index().Data as IEnumerable<SiteViewModel>;
			
			result.Single().Id.Should().Be(site1.Id.Value.ToString());
			result.Single().Name.Should().Be(site1.Description.Name);
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			const int expected = 14;

			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInSiteReader>();
			var target = new SitesController(siteRepository, numberOfAgentsQuery, null, null, null,null);

			var site = new Site("London");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.LoadAll()).Return(new[] { site });
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { site })).Return(new Dictionary<Guid, int>() { { site.Id.Value, expected } });
			var result = target.Index().Data as IEnumerable<SiteViewModel>;

			result.Single().NumberOfAgents.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetSingleSite()
		{
			var expected = Guid.NewGuid().ToString();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new SitesController(siteRepository, null, null, null, null,null);
			var site = new Site(expected);
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);

			var result = target.Get(site.Id.Value.ToString()).Data as SiteViewModel;

			result.Id.Should().Be.EqualTo(site.Id.Value.ToString());
			result.Name.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetOutOfAdherenceForSite()
		{
			const int expected = 1;
			var siteId = Guid.NewGuid();
			var siteAdherenceAggregator = MockRepository.GenerateMock<ISiteAdherenceAggregator>();
			siteAdherenceAggregator.Stub(x => x.Aggregate(siteId)).Return(expected);
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new SitesController(siteRepository, null, siteAdherenceAggregator, null, null,null);

			var result = target.GetOutOfAdherence(siteId.ToString()).Data as SiteOutOfAdherence;

			result.Id.Should().Be(siteId.ToString());
			result.OutOfAdherence.Should().Be(expected);
		}

		[Test]
		public void ShouldReturnEmptyWhenNoDataInBu()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			siteRepository.Stub(x => x.LoadAll()).Return(new ISite[] { });
			var target = new SitesController(siteRepository, null, null, null, null,null);

			var result = target.Index().Data as IEnumerable<SiteViewModel>;

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldGetBusinessUnitIdFromSiteId()
		{
			var site = new Site("s").WithId();
			var bu = new BusinessUnit("bu").WithId();
			site.SetBusinessUnit(bu);
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			siteRepository.Stub(x => x.Get(site.Id.GetValueOrDefault())).Return(site);
			var target = new SitesController(siteRepository, null, null, null, null,null);

			var result = target.GetBusinessUnitId(site.Id.ToString());
			result.Data.Should().Be(bu.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldGetFromReadModel()
		{
			var getAdherence = MockRepository.GenerateMock<IGetSiteAdherence>();
			var target = new SitesController(null, null, null, getAdherence, null,null);

			var sites = new List<SiteOutOfAdherence>()
						{
							new SiteOutOfAdherence(){Id="Site1",OutOfAdherence = 2},
							new SiteOutOfAdherence(){Id="Site2",OutOfAdherence = 5}
						};
		
			getAdherence.Stub(g => g.OutOfAdherence()).Return(sites);

			var result = target.GetOutOfAdherenceForAllSites().Data as IEnumerable<SiteOutOfAdherence>;

			result.Single(x => x.Id == sites[0].Id).OutOfAdherence.Should().Be(2);
			result.Single(x => x.Id == sites[1].Id).OutOfAdherence.Should().Be(5);

		}
	}
}
