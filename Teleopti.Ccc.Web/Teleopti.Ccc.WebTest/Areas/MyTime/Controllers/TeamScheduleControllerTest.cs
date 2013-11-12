using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class TeamScheduleControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnRequestPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var personPeriodProvider = MockRepository.GenerateMock<IPersonPeriodProvider>();
			var date = DateOnly.Today.AddDays(1);
			personPeriodProvider.Stub(x => x.HasPersonPeriod(date)).Return(true);
			var id = Guid.NewGuid();
			var target = new TeamScheduleController(viewModelFactory, MockRepository.GenerateMock<IDefaultTeamProvider>());

			viewModelFactory.Stub(x => x.CreateViewModel(date, id)).Return(new TeamScheduleViewModel());

			var result = target.Index(date, id);

			result.ViewName.Should().Be.EqualTo("TeamSchedulePartial");
			result.Model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseTodayWhenDateNotSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var personPeriodProvider = MockRepository.GenerateMock<IPersonPeriodProvider>();
			personPeriodProvider.Stub(x => x.HasPersonPeriod(DateOnly.Today)).Return(true);
			var target = new TeamScheduleController(viewModelFactory, MockRepository.GenerateMock<IDefaultTeamProvider>());

			target.Index(null, Guid.Empty);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, Guid.Empty));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseMyTeamsIdWhenNoIdSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var defaultTeamCalculator = MockRepository.GenerateMock<IDefaultTeamProvider>();
			var personPeriodProvider = MockRepository.GenerateMock<IPersonPeriodProvider>();
			personPeriodProvider.Stub(x => x.HasPersonPeriod(DateOnly.Today)).Return(true);
			var team = new Domain.AgentInfo.Team();
			team.SetId(Guid.NewGuid());
			defaultTeamCalculator.Stub(x => x.DefaultTeam(DateOnly.Today)).Return(team);

			var target = new TeamScheduleController(viewModelFactory, defaultTeamCalculator);

			target.Index(DateOnly.Today, null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, team.Id.Value));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnTeamsAsJson()
		{
			var teams = new[] {new SelectGroup()};
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today)).Return(teams);

			var target = new TeamScheduleController(viewModelFactory, null);

			var result = target.Teams(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectGroup>;
			data.Should().Have.SameValuesAs(teams);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeams()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var target = new TeamScheduleController(viewModelFactory, null);

			target.Teams(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today));
		}
	}
}
