using System;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Controllers;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Controllers
{
	[TestFixture]
	public class ReportControllerTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_viewModelFactory = _mocks.DynamicMock<IReportViewModelFactory>();
			_reportRequestValidator = _mocks.DynamicMock<IReportRequestValidator>();
			_target =
				new StubbingControllerBuilder().CreateController<ReportController>(new object[]
				                                                                   	{_viewModelFactory, _reportRequestValidator});
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		#endregion

		private ReportController _target;
		private MockRepository _mocks;
		private IReportViewModelFactory _viewModelFactory;
		private IReportRequestValidator _reportRequestValidator;

		[Test]
		public void ReportRequestShouldReturnJsonResult()
		{
			var reportRequestModel = new ReportRequestModel {ReportDate = new DateOnly(new DateTime(2011, 01, 19))};

			JsonResult result = _target.Report(reportRequestModel);

			result.Should().Be.OfType<JsonResult>();
		}

		[Test]
		public void ShouldCreatePortalModelAndView()
		{
			using (_mocks.Record())
			{
				Expect.Call(_viewModelFactory.CreateReportViewModel()).Return(new ReportViewModel());
			}
			using (_mocks.Playback())
			{
				ViewResult result = _target.Index();
				var model = result.Model as ReportViewModel;

				result.ViewName.Should().Be.Empty(); // default view
				model.Should().Not.Be.Null();
			}
		}


		[Test]
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			const string message = "Test model validation error";
			_target.ModelState.AddModelError("Test", message);

			JsonResult result = _target.Report(new ReportRequestModel());

			var data = result.Data as ModelStateResult;

			_target.Response.StatusCode.Should().Be(400);
			_target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldReturnErrorMessageWhenValidationFails()
		{
			const string message = "Error Message from validation";
			var reportRequestValidationResult = new ReportDataFetchResult
			                                    	{
			                                    		Errors = new[] {message}
			                                    	};
			using (_mocks.Record())
			{
				Expect.Call(_reportRequestValidator.FetchData(null)).IgnoreArguments().Return(reportRequestValidationResult);
			}
			using (_mocks.Playback())
			{
				JsonResult result = _target.Report(null);
				var data = result.Data as ModelStateResult;

				_target.Response.StatusCode.Should().Be(400);
				_target.Response.TrySkipIisCustomErrors.Should().Be.True();
				data.Errors.Single().Should().Be(message);
			}
		}
	}
}