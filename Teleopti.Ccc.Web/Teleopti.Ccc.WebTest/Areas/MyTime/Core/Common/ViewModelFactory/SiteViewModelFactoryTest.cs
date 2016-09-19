using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
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
		private ISiteProvider _siteProvider;

		public SiteViewModelFactoryTest()
		{
			_siteProvider = MockRepository.GenerateMock<ISiteProvider>();
		}

		[Test]
		public void ShouldCreateSiteOptionsViewModelForShiftTrade()
		{
			var site = new Site("mySite");
			site.SetBusinessUnit(new BusinessUnit("BU"));
			var sites = new[] { site };
			sites[0].SetId(Guid.NewGuid());

			_siteProvider.Stub(x => x.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(sites);

			var target = new SiteViewModelFactory(_siteProvider);

			var result = target.CreateSiteOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			result.Select(t => t.text).Should().Have.SameSequenceAs("BU/mySite");
		}

		[Test]
		public void ShouldGetTeamsUnderGivenSites()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			var teamId1 = Guid.NewGuid();
			var teamName1 = "team1";
			var teamId2 = Guid.NewGuid();
			var teamName2 = "team2";

			var team1 = new Team();
			team1.SetId(teamId1);
			team1.Description = new Description(teamName1);
			var team2 = new Team();
			team2.SetId(teamId2);
			team2.Description = new Description(teamName2);

			var siteIds = new List<Guid>() { siteId1, siteId2 };

			_siteProvider.Stub(x => x.GetTeamsUnderSite(siteId1)).Return(new List<ITeam> { team1 });
			_siteProvider.Stub(x => x.GetTeamsUnderSite(siteId2)).Return(new List<ITeam> { team2 });

			var target = new SiteViewModelFactory(_siteProvider);

			var result = target.GetTeams(siteIds);
			result.ToList()[0].text.Should().Be.EqualTo(teamName1);
			result.ToList()[1].text.Should().Be.EqualTo(teamName2);
	}
}
}
