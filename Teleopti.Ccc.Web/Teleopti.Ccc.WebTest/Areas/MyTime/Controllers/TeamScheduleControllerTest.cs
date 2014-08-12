using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
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
			var now = MockRepository.GenerateMock<INow>();
			var date = DateOnly.Today.AddDays(1);
			personPeriodProvider.Stub(x => x.HasPersonPeriod(date)).Return(true);
			now.Stub(x => x.UtcDateTime()).Return(date.Date.ToUniversalTime());
			var id = Guid.NewGuid();
			var target = new TeamScheduleController(now,viewModelFactory, MockRepository.GenerateMock<IDefaultTeamProvider>());

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
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(DateTime.UtcNow); 
			personPeriodProvider.Stub(x => x.HasPersonPeriod(DateOnly.Today)).Return(true);
			var target = new TeamScheduleController(now,viewModelFactory, MockRepository.GenerateMock<IDefaultTeamProvider>());

			target.Index(null, Guid.Empty);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, Guid.Empty));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseMyTeamsIdWhenNoIdSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var defaultTeamCalculator = MockRepository.GenerateMock<IDefaultTeamProvider>();
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(DateTime.UtcNow);
			var personPeriodProvider = MockRepository.GenerateMock<IPersonPeriodProvider>();
			personPeriodProvider.Stub(x => x.HasPersonPeriod(DateOnly.Today)).Return(true);
			var team = new Domain.AgentInfo.Team();
			team.SetId(Guid.NewGuid());
			defaultTeamCalculator.Stub(x => x.DefaultTeam(DateOnly.Today)).Return(team);

			var target = new TeamScheduleController(now,viewModelFactory, defaultTeamCalculator);

			target.Index(DateOnly.Today, null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(DateOnly.Today, team.Id.Value));
		}	
	}
}
