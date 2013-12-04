﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.ViewModelFactory
{
	[TestFixture]
	public class TeamViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateTeamOptionsViewModelIfNotHaveViewAllGroupPagesPermission()
		{
			var teams = new[] { new Team() };
			teams[0].SetId(Guid.NewGuid());
			teams[0].Description = new Description("team");
			teams[0].Site = new Site("site");
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(teams);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages)).Return(false);
			var target = new TeamViewModelFactory(teamProvider, permissionProvider, null);

			var result = target.CreateTeamOrGroupOptionsViewModel(DateOnly.Today);

			result.Select(t => t.text).Should().Have.SameSequenceAs("site/team");
		}


		[Test]
		public void ShouldCreateGroupPageOptionsViewModelForBusinessHierarchyIfHaveViewAllGroupPagesPermission()
		{
			var teamId = Guid.NewGuid();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages)).Return(true);
			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, DateOnly.Today, null)).IgnoreArguments().Return(true);
			var groupingReadOnlyRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			var pageId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");
			var readOnlyGroupPage = new ReadOnlyGroupPage { PageId = pageId, PageName = "xxMain" };
			groupingReadOnlyRepository.Stub(x => x.AvailableGroupPages()).Return(new List<ReadOnlyGroupPage> { readOnlyGroupPage });
			groupingReadOnlyRepository.Stub(x => x.AvailableGroups(DateOnly.Today)).IgnoreArguments().Return(new List<ReadOnlyGroupDetail> { new ReadOnlyGroupDetail { PageId = pageId, GroupName = "team", GroupId = teamId } });
			var target = new TeamViewModelFactory(null, permissionProvider, groupingReadOnlyRepository);

			var result = target.CreateTeamOrGroupOptionsViewModel(DateOnly.Today) as IEnumerable<ISelectGroup>;
			result.FirstOrDefault().children.FirstOrDefault().id.Should().Be.EqualTo(teamId.ToString());
			result.FirstOrDefault().children.FirstOrDefault().text.Should().Be.EqualTo("team");
			result.FirstOrDefault().text.Should().Be.EqualTo("Business Hierarchy");
		}

		[Test]
		public void ShouldCreateGroupPageOptionsViewModelForOtherTypesIfHaveViewAllGroupPagesPermission()
		{
			var teamId = Guid.NewGuid();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages)).Return(true);
			permissionProvider.Stub(x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, DateOnly.Today, null)).IgnoreArguments().Return(true);
			var groupingReadOnlyRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			var pageId = Guid.NewGuid();
			var readOnlyGroupPage = new ReadOnlyGroupPage { PageId = pageId, PageName = "xxContract" };
			groupingReadOnlyRepository.Stub(x => x.AvailableGroupPages()).Return(new List<ReadOnlyGroupPage> { readOnlyGroupPage });
			groupingReadOnlyRepository.Stub(x => x.AvailableGroups(DateOnly.Today)).IgnoreArguments().Return(new List<ReadOnlyGroupDetail> { new ReadOnlyGroupDetail { PageId = pageId, GroupName = "full time", GroupId = teamId } });
			var target = new TeamViewModelFactory(null, permissionProvider, groupingReadOnlyRepository);

			var result = target.CreateTeamOrGroupOptionsViewModel(DateOnly.Today) as IEnumerable<ISelectGroup>;
			result.FirstOrDefault().children.FirstOrDefault().id.Should().Be.EqualTo(teamId.ToString());
			result.FirstOrDefault().children.FirstOrDefault().text.Should().Be.EqualTo("Contract/full time");
			result.FirstOrDefault().text.Should().Be.EqualTo("Contract");
		}
	}
}
