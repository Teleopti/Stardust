using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Controllers
{
	[TestFixture]
	public class TeamScheduleControllerTest
	{
		[Test]
		public void ShouldReturnRequestPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var target = new TeamScheduleController(viewModelFactory, MockRepository.GenerateMock<IDefaultTeamCalculator>(), null);
			var date = DateOnly.Today.AddDays(1);
			var id = Guid.NewGuid();

			viewModelFactory.Stub(x => x.CreateViewModel(date, id)).Return(new TeamScheduleViewModel());

			var result = target.Index(date, id);

			result.ViewName.Should().Be.EqualTo("TeamSchedulePartial");
			result.Model.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseTodayWhenDateNotSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var target = new TeamScheduleController(viewModelFactory, MockRepository.GenerateMock<IDefaultTeamCalculator>(), null);

			target.Index(null, Guid.Empty);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, Guid.Empty));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseMyTeamsIdWhenNoIdSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var defaultTeamCalculator = MockRepository.GenerateMock<IDefaultTeamCalculator>();
			var team = new Team();
			team.SetId(Guid.NewGuid());
			defaultTeamCalculator.Stub(x => x.Calculate(DateOnly.Today)).Return(team);

			var target = new TeamScheduleController(viewModelFactory, defaultTeamCalculator, null);

			target.Index(DateOnly.Today, null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, team.Id.Value));
		}

		[Test]
		public void ShouldReturnTeamsAsJson()
		{
			var teams = new[] {new SelectBoxOption()};
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateTeamOptionsViewModel(DateOnly.Today)).Return(teams);

			var target = new TeamScheduleController(viewModelFactory, null, null);

			var result = target.Teams(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectBoxOption>;
			data.Should().Have.SameValuesAs(teams);
		}

		[Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeams()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var target = new TeamScheduleController(viewModelFactory, null, null);

			target.Teams(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOptionsViewModel(DateOnly.Today));
		}
	}
}
