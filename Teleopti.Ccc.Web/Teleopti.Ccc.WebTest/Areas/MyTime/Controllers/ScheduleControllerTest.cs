using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;
using Teleopti.Ccc.Web.Core;


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
		public void ShouldReturnRequestData()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new RequestsViewModel();
			viewModelFactory.Expect(m => m.CreatePageViewModel()).Return(model);
			using (var target = new ScheduleController(viewModelFactory, null, null))
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
			using (var target = new ScheduleController(null, overtimeAvailabilityPersister, null))
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

			var target = new ScheduleController(null, overtimeAvailabilityPersister, null);
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

			using (var target = new ScheduleController(null, overtimeAvailabilityPersister, null))
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
			using (var target = new ScheduleController(null, null, absenceReportPersister))
			{
				var model = target.ReportAbsence(input).Data as AbsenceReportViewModel;
				model.Should().Be.SameInstanceAs(absenceReportViewModel);
			}
		}
	}
}