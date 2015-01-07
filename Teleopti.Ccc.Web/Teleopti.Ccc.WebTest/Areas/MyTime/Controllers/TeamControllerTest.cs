using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class TeamControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnTeamsAndOrGroupingssAsJson()
		{
			var teams = new[] { new SelectGroup() };
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today)).Return(teams);

			var target = new TeamController(viewModelFactory, new Now());

			var result = target.TeamsAndOrGroupings(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectGroup>;
			data.Should().Have.SameValuesAs(teams);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeamsAndOrGroupings()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			var target = new TeamController(viewModelFactory, new Now());

			target.TeamsAndOrGroupings(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today));
		}

		[Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeams()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			var target = new TeamController(viewModelFactory, new Now());

			target.TeamsForShiftTrade(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
		}

		[Test]
		public void ShouldReturnTeamOptionsAsJsonForShiftTrade()
		{
			var teams = new[] { new SelectGroup() };
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			viewModelFactory.Stub(
				x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			                .Return(teams);

			var target = new TeamController(viewModelFactory, new Now());

			var result = target.TeamsForShiftTrade(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectGroup>;
			data.Should().Have.SameValuesAs(teams);
		}
	}
}
