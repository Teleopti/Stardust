using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnRequestPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			var date = DateOnly.Today.AddDays(1);
			now.Stub(x => x.UtcDateTime()).Return(date.Date.ToUniversalTime());
			var id = Guid.NewGuid();
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new TeamScheduleController(now, viewModelFactory, MockRepository.GenerateMock<IDefaultTeamProvider>(),
				MockRepository.GenerateMock<ITimeFilterHelper>(), toggleManager,
				MockRepository.GenerateMock<ILoggedOnUser>(),
				MockRepository.GenerateMock<ITeamScheduleViewModelReworkedFactory>());

			viewModelFactory.Stub(x => x.CreateViewModel()).Return(new TeamScheduleViewModel());
			var result = target.Index(date, id);

			result.ViewName.Should().Be.EqualTo("TeamSchedulePartial");
			result.Model.Should().Not.Be.Null();
		}
	}
}