using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.Anywhere)]
	public class ReportController : Controller
	{
		private readonly IReportRequestValidator _reportRequestValidator;
		private readonly IReportViewModelFactory _reportsViewModelFactory;

		public ReportController(IReportViewModelFactory reportsViewModelFactory,
		                        IReportRequestValidator reportRequestValidator)
		{
			_reportsViewModelFactory = reportsViewModelFactory;
			_reportRequestValidator = reportRequestValidator;
		}

		[UnitOfWorkAction]
		public ViewResult Index()
		{
			ViewBag.Title = "Anywhere";
			ReportViewModel reportModel = _reportsViewModelFactory.CreateReportViewModel();

			return View(reportModel);
		}

		[HttpPost, UnitOfWorkAction]
		public JsonResult Report(ReportRequestModel reportRequest)
		{
			if (!ModelState.IsValid)
			{
				return PrepareAndReturnJsonError(null);
			}

			ReportDataFetchResult validationResult = _reportRequestValidator.FetchData(reportRequest);
			if (!validationResult.IsValid())
			{
				var jsonResult = new JsonResult {Data = new ModelStateResult {Errors = validationResult.Errors.ToArray()}};
				return PrepareAndReturnJsonError(jsonResult);
			}

			var reportModel = _reportsViewModelFactory.GenerateReportDataResponse(validationResult.GenerationRequest);

			return Json(reportModel);
		}

		private JsonResult PrepareAndReturnJsonError(JsonResult result)
		{
			Response.TrySkipIisCustomErrors = true;
			Response.StatusCode = 400;
			return result ?? ModelState.ToJson();
		}
	}
}