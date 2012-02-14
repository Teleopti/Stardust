using System.Collections.Specialized;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Controllers
{
	[TestFixture]
	public class ScheduleControllerTest
	{
		[Test]
		public void ShouldRedirectToWeekActionFromDefault()
		{
			var target = new ScheduleController(null);
			var result = target.Index() as RedirectToRouteResult;
			result.RouteValues["action"].Should().Be.EqualTo("Week");
		}

		[Test]
		public void ShouldViewScheduleForCurrentWeek()
		{
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(DateOnly.Today)).Return(new WeekScheduleViewModel());
			using (var target = new ScheduleController(viewModelFactory))
			{
				var result = target.Week(null) as ViewResult;

				var model = result.Model as WeekScheduleViewModel;
				result.ViewName.Should().Be.EqualTo("WeekPartial");
				model.Should().Not.Be.Null();				
			}
		}

		[Test]
		public void ShouldReturnJsonScheduleForCurrentWeek()
		{
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(DateOnly.Today)).Return(new WeekScheduleViewModel());
			using (var target = new ScheduleController(viewModelFactory))
			{
				new StubbingControllerBuilder().InitializeController(target);
				target.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(new NameValueCollection { { "Accept", "application/json" } });

				var result = target.Week(null) as JsonResult;

				var data = result.Data as WeekScheduleViewModel;
				data.Should().Not.Be.Null();			
			}
		}

		[Test]
		public void ShouldViewScheduleForGivenDate()
		{
			var date = new DateOnly(2011, 01, 01);
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(date)).Return(new WeekScheduleViewModel());
			using (var target = new ScheduleController(viewModelFactory))
			{
				var result = target.Week(date) as ViewResult;

				var model = result.Model as WeekScheduleViewModel;
				result.ViewName.Should().Be.EqualTo("WeekPartial");
				model.Should().Not.Be.Null();				
			}
		}
	}
}