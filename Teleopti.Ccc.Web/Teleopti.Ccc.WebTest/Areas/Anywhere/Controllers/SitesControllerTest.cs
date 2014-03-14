using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

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
			var target = new SitesController(siteRepository, numberOfAgentsQuery);
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
			var target = new SitesController(siteRepository, numberOfAgentsQuery);

			var site = new Site("London");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.LoadAll()).Return(new[] { site });
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { site })).Return(new Dictionary<Guid, int>() { { site.Id.Value, expected } });
			var result = target.Index().Data as IEnumerable<SiteViewModel>;

			result.Single().NumberOfAgents.Should().Be.EqualTo(expected);
		}
	}
}
