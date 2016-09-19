﻿using System;
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
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class SiteProviderTest
	{
		private ITeamRepository _teamRepository;

		public SiteProviderTest()
		{
			_teamRepository = MockRepository.GenerateMock<ITeamRepository>();
		}

		[Test]
		public void ShouldFilterPermittedTeamsWhenQueryingAll()
		{
			var repository = MockRepository.GenerateMock<ISiteRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var sites = new ISite[] { new Site("site1"), new Site("site2"),  };

			repository.Stub(x => x.LoadAllOrderByName()).Return(sites);
			permissionProvider.Stub(x => x.HasSitePermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, sites.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasSitePermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, sites.ElementAt(1))).Return(true);

			var target = new SiteProvider(repository, permissionProvider, null);

			var result = target.GetPermittedSites(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(sites.ElementAt(1));
		}

		[Test]
		public void ShouldGetTeamsUnderSite()
		{
			var site = new Site("mySite");
			site.SetId(Guid.NewGuid());
			var team1 = new Team();
			team1.Site = site;
			var team2 = new Team();
			team2.Site = site;

			_teamRepository.Stub(x => x.FindTeamsForSite(site.Id.Value)).Return(new List<Team>() { team1, team2 });

			var target = new SiteProvider(null, null, _teamRepository);

			var result = target.GetTeamsUnderSite(site.Id.Value);
			result.ToList()[0].Should().Be.EqualTo(team1);
			result.ToList()[1].Should().Be.EqualTo(team2);
	}
}
}
