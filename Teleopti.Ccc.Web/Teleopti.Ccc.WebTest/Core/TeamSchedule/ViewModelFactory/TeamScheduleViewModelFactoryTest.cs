using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
			var target = new TeamScheduleViewModelFactory(mapper, null);
			var viewModel = new TeamScheduleViewModel();
			var data = new TeamScheduleDomainData();
			var id = Guid.NewGuid();

			mapper.Stub(x => x.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(DateOnly.Today, id))).Return(data);
			mapper.Stub(x => x.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today, id);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateTeamOptionsViewModel()
		{
			var teams = new[] {new Team()};
			teams[0].SetId(Guid.NewGuid());
			teams[0].Description = new Description("team");
			teams[0].Site = new Site("site");
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(teams);
			var target = new TeamScheduleViewModelFactory(null, teamProvider);

			var result = target.CreateTeamOptionsViewModel(DateOnly.Today);

			var selectOptionGroup = new SelectOptionGroup {text = "site", children = new ISelectOptionItem[] {new SelectOptionItem {id = teams[0].Id.GetValueOrDefault().ToString(), text = "team"}}};
			var expected = new[]
			               	{
			               		selectOptionGroup
			               	};
			result.Select(t => t.children[0].text).Should().Have.SameSequenceAs(expected.Select(t => t.children[0].text));
			result.Select(t => t.text).Should().Have.SameSequenceAs(expected.Select(t => t.text));
		}

	}
}
