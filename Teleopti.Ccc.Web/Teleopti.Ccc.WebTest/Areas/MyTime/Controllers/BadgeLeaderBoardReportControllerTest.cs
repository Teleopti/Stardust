using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class BadgeLeaderBoardRpeortControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForBadgeLeaderBoardReport_ShouldReturnPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IBadgeLeaderBoardReportViewModelFactory>();
			var target = new BadgeLeaderBoardReportController(viewModelFactory, null);
			var model = new BadgeLeaderBoardReportViewModel();
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			viewModelFactory.Stub(x => x.CreateBadgeLeaderBoardReportViewModel(option)).Return(model);

			var result = target.Overview(option);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void IndexShouldReturnPartialView()
		{
			var controller = new BadgeLeaderBoardReportController(null, null);
			controller.Index().Should().Be.OfType<ViewResult>();
		}
	}
}