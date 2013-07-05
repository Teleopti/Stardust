using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ScheduleControllerTest
	{
		[Test]
		public void ShouldRedirectToWeekActionFromDefault()
		{
			using (var target = new ScheduleController(null, null, null))
			{
				var result = target.Index() as RedirectToRouteResult;
				result.RouteValues["action"].Should().Be.EqualTo("Week");				
			}
		}

		[Test]
		public void ShouldReturnJsonScheduleForCurrentWeek()
		{
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.DateOnly()).Return(new DateOnly(2012, 8, 1));
			viewModelFactory.Stub(x => x.CreateWeekViewModel(now.DateOnly())).Return(new WeekScheduleViewModel());
			
			using (var target = new ScheduleController(viewModelFactory, null, now))
			{
				new StubbingControllerBuilder().InitializeController(target);
				target.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(new NameValueCollection { { "Accept", "application/json" } });

				var result = target.FetchData(null);

				var data = result.Data as WeekScheduleViewModel;
				data.Should().Not.Be.Null();			
			}
		}

		[Test]
		public void ShouldViewScheduleForGivenDate()
		{
			var date = new DateOnly(2011, 01, 01);
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(date)).Return(new WeekScheduleViewModel());
			using (var target = new ScheduleController(viewModelFactory, null, now))
			{
				var result = target.FetchData(date);

				var model = result.Data as WeekScheduleViewModel;
				model.Should().Not.Be.Null();				
			}
		}

		[Test]
		public void ShouldReturnRequestData()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new RequestsViewModel();
			viewModelFactory.Expect(m => m.CreatePageViewModel()).Return(model);
			using (var target = new ScheduleController(null, viewModelFactory, null))
			{
				var result = target.Week() as ViewResult;
				result.Model.Should().Be.SameInstanceAs(model);
			}
		}
	}

	[TestFixture]
	public class UserInfoControllerTest
	{
		[Test]
		public void Culture_WhenUsersCultureHasMondayAsFirstDay_ShouldSetWeekStartTo1()
		{
			var cultureStub = MockRepository.GenerateStub<IUserCulture>();
			cultureStub.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var target = new UserInfoController(cultureStub);
			
			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.WeekStart,Is.EqualTo(1));
			Assert.That(result,Is.Not.Null);
		}
	}
}