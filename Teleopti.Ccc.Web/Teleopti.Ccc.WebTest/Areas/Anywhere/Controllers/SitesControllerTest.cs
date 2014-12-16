﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class SitesControllerTest
	{
		[Test]
		public void ShouldGetSitesForMyBusinessUnit()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInSiteReader>();
			var target = new SitesController(siteRepository, numberOfAgentsQuery,null);
			var site = new Site("London");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.LoadAll()).Return(new[] { site });
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { site })).Return(new Dictionary<Guid, int>() { { site.Id.Value, 0 } });
			
			var result = target.Index().Data as IEnumerable<SiteViewModel>;

			result.Single().Id.Should().Be(site.Id.Value.ToString());
			result.Single().Name.Should().Be(site.Description.Name);
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			const int expected = 14;

			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInSiteReader>();
			var target = new SitesController(siteRepository, numberOfAgentsQuery, null);

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
			var target = new SitesController(siteRepository, null, null);
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
			var target = new SitesController(siteRepository, null, siteAdherenceAggregator);

			var result = target.GetOutOfAdherence(siteId.ToString()).Data as SiteOutOfAdherence;

			result.Id.Should().Be(siteId.ToString());
			result.OutOfAdherence.Should().Be(expected);
		}

		[Test]
		public void ShouldReturnAnEmptySiteWhenNoDataInBu()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new SitesController(siteRepository, null, null);

			var result = target.Index().Data as SiteViewModel;
			result.Id.Should().Be("");
			result.Name.Should().Be("");
			result.NumberOfAgents.Should().Be(0);
		}

		[Test]
		public void ShouldGetBusinessUnitIdFromSiteId()
		{
			var site = new Site(" ").WithId();
			var bu = new BusinessUnit(" ").WithId();
			site.SetBusinessUnit(bu);
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			siteRepository.Stub(x => x.Get(site.Id.GetValueOrDefault())).Return(site);
			var target = new SitesController(siteRepository, null, null);

			var result = target.GetBusinessUnitId(site.Id.ToString());
			result.Data.Should().Be(bu.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldGetAdherenceForAllSites()
		{
			var site1 = new Site("1");
			site1.SetId(Guid.NewGuid());
			var site2 = new Site("2");
			site2.SetId(Guid.NewGuid());
			var siteAdherenceAggregator = MockRepository.GenerateMock<ISiteAdherenceAggregator>();
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var target = new SitesController(siteRepository, null, siteAdherenceAggregator);
			siteRepository.Stub(x => x.LoadAll()).Return(new[] {site1, site2});
			siteAdherenceAggregator.Stub(x => x.Aggregate(site1.Id.Value)).Return(1);
			siteAdherenceAggregator.Stub(x => x.Aggregate(site2.Id.Value)).Return(2);

			var result = target.GetOutOfAdherenceForAllSites().Data as IEnumerable<SiteOutOfAdherence>;

			result.Single(x => x.Id == site1.Id.Value.ToString()).OutOfAdherence.Should().Be(1);
			result.Single(x => x.Id == site2.Id.Value.ToString()).OutOfAdherence.Should().Be(2);
		}
	}
}
