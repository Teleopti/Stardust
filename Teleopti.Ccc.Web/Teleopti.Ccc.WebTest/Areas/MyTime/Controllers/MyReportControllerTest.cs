using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MyReportControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForMyReport_ShouldReturnPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMyReportViewModelFactory>();
			var target = new MyReportController(viewModelFactory);
			var model = new DailyMetricsViewModel();
			var date = DateOnly.Today;

			viewModelFactory.Stub(x => x.CreateDailyMetricsViewModel(date)).Return(model);

			var result = target.OnDates(date);
				
			result.Data.Should().Be.SameInstanceAs(model);
		}
	}
}