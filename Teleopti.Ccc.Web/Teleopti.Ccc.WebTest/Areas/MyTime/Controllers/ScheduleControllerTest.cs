using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ScheduleControllerTest
	{
		[Test]
		public void ShouldRedirectToWeekActionFromDefault()
		{
			using (var target = new ScheduleController(null, null, null, null, null))
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
			now.Stub(x => x.UtcDateTime()).Return(new DateTime(2012, 8, 1));
			viewModelFactory.Stub(x => x.CreateWeekViewModel(now.LocalDateOnly())).Return(new WeekScheduleViewModel());

			using (var target = new ScheduleController(viewModelFactory, null, now, null, null))
			{
				new StubbingControllerBuilder().InitializeController(target);
				target.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(new NameValueCollection { { "Accept", "application/json" } });

				var result = target.FetchData(null);

				var data = result.Data as WeekScheduleViewModel;
				data.Should().Not.Be.Null();			
			}
		}
        
        [Test]
        public void ShouldReturnJsonScheduleForCurrentMonth()
        {
            var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
            var now = MockRepository.GenerateMock<INow>();
            now.Stub(x => x.UtcDateTime()).Return(new DateTime(2012, 8, 1));
            viewModelFactory.Stub(x => x.CreateMonthViewModel(now.LocalDateOnly())).Return(new MonthScheduleViewModel());

            using (var target = new ScheduleController(viewModelFactory, null, now, null, null))
            {
                new StubbingControllerBuilder().InitializeController(target);
                target.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(new NameValueCollection { { "Accept", "application/json" } });

                var result = target.FetchMonthData(null);

                var data = result.Data as MonthScheduleViewModel;
                data.Should().Not.Be.Null();
            }
        }
		[Test]
		public void ShouldViewWeekScheduleForGivenDate()
		{
			var date = new DateOnly(2011, 01, 01);
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(date)).Return(new WeekScheduleViewModel());
			using (var target = new ScheduleController(viewModelFactory, null, now, null, null))
			{
				var result = target.FetchData(date);

				var model = result.Data as WeekScheduleViewModel;
				model.Should().Not.Be.Null();				
			}
		}

        [Test]
        public void ShouldViewMonthScheduleForGivenDate()
        {
            var date = new DateOnly(2011, 01, 01);
            var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
            var now = MockRepository.GenerateMock<INow>();
            viewModelFactory.Stub(x => x.CreateMonthViewModel(date)).Return(new MonthScheduleViewModel());
            using (var target = new ScheduleController(viewModelFactory, null, now, null, null))
            {
                var result = target.FetchMonthData(date);

                var model = result.Data as MonthScheduleViewModel;
                model.Should().Not.Be.Null();
            }
        }

		[Test]
		public void ShouldReturnRequestData()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new RequestsViewModel();
			viewModelFactory.Expect(m => m.CreatePageViewModel()).Return(model);
			using (var target = new ScheduleController(null, viewModelFactory, null, null, null))
			{
				var result = target.Week() as ViewResult;
				result.Model.Should().Be.SameInstanceAs(model);
			}
		}

		[Test]
		public void ShouldPersistOvertimeAvailability()
		{
			var overtimeAvailabilityPersister = MockRepository.GenerateMock<IOvertimeAvailabilityPersister>();
			var input = new OvertimeAvailabilityInput();
			var overtimeAvailabilityViewModel = new OvertimeAvailabilityViewModel();
			overtimeAvailabilityPersister.Stub(x => x.Persist(input)).Return(overtimeAvailabilityViewModel);
			using (var target = new ScheduleController(null, null, null, overtimeAvailabilityPersister, null))
			{
				var model = target.OvertimeAvailability(input).Data as OvertimeAvailabilityViewModel;
				model.Should().Be.SameInstanceAs(overtimeAvailabilityViewModel);
			}
		}

		[Test]
		public void ShouldHandleModelErrorInPersistOvertimeAvailabilityInput()
		{
			var overtimeAvailabilityPersister = MockRepository.GenerateMock<IOvertimeAvailabilityPersister>();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var input = new OvertimeAvailabilityInput();

			var target = new ScheduleController(null, null, null, overtimeAvailabilityPersister, null);
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);
			target.ModelState.AddModelError("Error", "Error");

			var result = target.OvertimeAvailability(input);
			var data = result.Data as ModelStateResult;
			data.Errors.Should().Contain("Error");
		}

		[Test]
		public void ShouldDeleteOvertimeAvailability()
		{
			var overtimeAvailabilityPersister = MockRepository.GenerateMock<IOvertimeAvailabilityPersister>();
			var date = DateOnly.Today;
			var overtimeAvailabilityViewModel = new OvertimeAvailabilityViewModel();
			overtimeAvailabilityPersister.Stub(x => x.Delete(date)).Return(overtimeAvailabilityViewModel);

			using (var target = new ScheduleController(null, null, null, overtimeAvailabilityPersister, null))
			{
				var model = target.DeleteOvertimeAvailability(date).Data as OvertimeAvailabilityViewModel;
				model.Should().Be.SameInstanceAs(overtimeAvailabilityViewModel);
			}
		}

		[Test]
		public void ShouldPersistAbsenceReport()
		{
			var absenceReportPersister = MockRepository.GenerateMock<IAbsenceReportPersister>();
			var input = new AbsenceReportInput();
			var absenceReportViewModel = new AbsenceReportViewModel();
			absenceReportPersister.Stub(x => x.Persist(input)).Return(absenceReportViewModel);
			using (var target = new ScheduleController(null, null, null, null, absenceReportPersister))
			{
				var model = target.ReportAbsence(input).Data as AbsenceReportViewModel;
				model.Should().Be.SameInstanceAs(absenceReportViewModel);
			}
		}
	}
}