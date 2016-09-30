using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class SiteProviderTest
	{
		private ITeamRepository _teamRepository;
		private IPermissionProvider _permissionProvider;

		public SiteProviderTest()
		{
			_teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
		}

		[Test]
		public void ShouldFilterPermittedTeamsWhenQueryingAll()
		{
			var repository = MockRepository.GenerateMock<ISiteRepository>();
			var sites = new ISite[] { new Site("site1"), new Site("site2"),  };
			sites[0].SetId(Guid.NewGuid());
			sites[1].SetId(Guid.NewGuid());
			var team1 = new Team();
			team1.Site = sites[0];
			var team2 = new Team();
			team2.Site = sites[1];

			repository.Stub(x => x.LoadAllOrderByName()).Return(sites);
			_teamRepository.Stub(x => x.FindTeamsForSite(sites[0].Id.Value)).Return(new List<Team>() { team1 });
			_teamRepository.Stub(x => x.FindTeamsForSite(sites[1].Id.Value)).Return(new List<Team>() { team2 });
			_permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today, team1)).Return(false);
			_permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today, team2)).Return(true);

			var target = new SiteProvider(repository, _permissionProvider, _teamRepository);

			var result = target.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);

			result.Single().Should().Be(sites.ElementAt(1));
		}

		[Test]
		public void ShouldGetPermittedTeamsUnderSite()
		{
			var site = new Site("mySite");
			site.SetId(Guid.NewGuid());
			var team1 = new Team();
			team1.Site = site;
			var team2 = new Team();
			team2.Site = site;

			_teamRepository.Stub(x => x.FindTeamsForSite(site.Id.Value)).Return(new List<Team>() { team1, team2 });
			_permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today, team1)).Return(false);
			_permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today, team2)).Return(true);

			var target = new SiteProvider(null, _permissionProvider, _teamRepository);

			var result = target.GetPermittedTeamsUnderSite(site.Id.Value, DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			result.ToList().Count.Should().Be.EqualTo(1);
			result.ToList()[0].Should().Be.EqualTo(team2);
	}
}
}
