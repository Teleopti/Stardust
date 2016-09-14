using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.ViewModelFactory
{
	[TestFixture]
	public class SiteViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateSiteOptionsViewModelForShiftTrade()
		{
			var site = new Site("mySite");
			site.SetBusinessUnit(new BusinessUnit("BU"));
			var sites = new[] { site };
			sites[0].SetId(Guid.NewGuid());
			var siteProvider = MockRepository.GenerateMock<ISiteProvider>();

			siteProvider.Stub(
				x => x.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(sites);

			var target = new SiteViewModelFactory(siteProvider);

			var result = target.CreateSiteOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);

			result.Select(t => t.text).Should().Have.SameSequenceAs("BU/mySite");
		}

		[Test]
		public void ShouldGetAllTeamIdsOfTwoSites()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var teamId3 = Guid.NewGuid();
			var teamId4 = Guid.NewGuid();

			var siteIds = new List<Guid>() {siteId1, siteId2};

			var siteProvider = MockRepository.GenerateMock<ISiteProvider>();
			siteProvider.Stub(x => x.GetTeamIdsUnderSite(siteId1)).Return(new List<Guid> {teamId1, teamId2});
			siteProvider.Stub(x => x.GetTeamIdsUnderSite(siteId2)).Return(new List<Guid> {teamId3, teamId4});

			var target = new SiteViewModelFactory(siteProvider);

			var result = target.GetTeamIds(siteIds);
			result.ToList()[0].Should().Be.EqualTo(teamId1);
			result.ToList()[1].Should().Be.EqualTo(teamId2);
			result.ToList()[2].Should().Be.EqualTo(teamId3);
			result.ToList()[3].Should().Be.EqualTo(teamId4);
		}
	}
}
