using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	public class TeamScheduleViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new TeamScheduleViewModelFactory(mapper, null,new FakePermissionProvider(), null);
			var viewModel = new TeamScheduleViewModel();
			var data = new TeamScheduleDomainData();
			var id = Guid.NewGuid();

			mapper.Stub(x => x.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, id))).Return(data);
			mapper.Stub(x => x.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today, id);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateTeamOptionsViewModelIfNotHaveViewAllGroupPagesPermission()
		{
			var teams = new[] {new Team()};
			teams[0].SetId(Guid.NewGuid());
			teams[0].Description = new Description("team");
			teams[0].Site = new Site("site");
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(teams);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages)).Return(false);
			var target = new TeamScheduleViewModelFactory(null, teamProvider, permissionProvider, null);

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
			var target = new TeamScheduleViewModelFactory(null, null, permissionProvider, groupingReadOnlyRepository);

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
			var target = new TeamScheduleViewModelFactory(null, null, permissionProvider, groupingReadOnlyRepository);

			var result = target.CreateTeamOrGroupOptionsViewModel(DateOnly.Today) as IEnumerable<ISelectGroup>;
			result.FirstOrDefault().children.FirstOrDefault().id.Should().Be.EqualTo(teamId.ToString());
			result.FirstOrDefault().children.FirstOrDefault().text.Should().Be.EqualTo("Contract/full time");
			result.FirstOrDefault().text.Should().Be.EqualTo("Contract");
		}

		[Test]
		public void PermissionForShiftTrade_WhenAgentHasNoPermissionToViewShiftTrade_ShouldBFalse()
		{
			var target = new TeamScheduleViewModelFactory(createMappingEngine(), null, new FakePermissionProvider(), null);

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);			
		}

		[Test]
		public void PermissionForShiftTrade_WhenAgentHasPermissionToViewShiftTrade_ShouldBeTrue()
		{
			var target = new TeamScheduleViewModelFactory(createMappingEngine(), null, new FakePermissionProvider(), null);

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);
		}

		private static IMappingEngine createMappingEngine()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var viewModel = new TeamScheduleViewModel();

			mapper.Stub(x => x.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, new Guid())))
				  .Return(new TeamScheduleDomainData())
			      .IgnoreArguments();
			mapper.Stub(x => x.Map<TeamScheduleDomainData, TeamScheduleViewModel>(null))
				.IgnoreArguments()
				.Return(viewModel);
			return mapper;
		}
	}
}
