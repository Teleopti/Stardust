using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
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
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new BadgeLeaderBoardReportController(viewModelFactory, null, toggleManager);
			var model = new BadgeLeaderBoardReportViewModel();
			var date = DateOnly.Today;

			viewModelFactory.Stub(x => x.CreateBadgeLeaderBoardReportViewModel(date)).Return(model);

			var result = target.Overview(date);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void IndexShouldReturnPartialView()
		{
			var controller = new BadgeLeaderBoardReportController(null, null, null);
			controller.Index().Should().Be.OfType<ViewResult>();
		}
	}
}