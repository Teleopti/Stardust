using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[MyReportPermission]
	public class MyReportController : Controller
	{
	    private readonly IToggleManager _toggleManager;
	    private readonly IMyReportViewModelFactory _myReportViewModelFactory;
		private readonly IUserCulture _userCulture;

        public MyReportController(IMyReportViewModelFactory myReportViewModelFactory, IUserCulture userCulture, IToggleManager toggleManager)
		{
			_myReportViewModelFactory = myReportViewModelFactory;
			_userCulture = userCulture;
            _toggleManager = toggleManager;
		}

		[EnsureInPortal]
		public ViewResult Index()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			return View("MyReportPartial");
		}

		[HttpGet]
		[UnitOfWorkAction]
		public JsonResult Overview(DateOnly date)
		{
			return Json(_myReportViewModelFactory.CreateDailyMetricsViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[EnsureInPortal]
		public ViewResult Adherence()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			return View("AdherencePartial");
		}

		[HttpGet]
		[UnitOfWorkAction]
		public JsonResult AdherenceDetails(DateOnly date)
		{
			return Json(_myReportViewModelFactory.CreateDetailedAherenceViewModel(date), JsonRequestBehavior.AllowGet);
		}

        [EnsureInPortal]
		[MyReportQueueMetricsPermission]
        public ViewResult QueueMetrics()
        {
            if(!_toggleManager.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254))
                throw new HttpException(404, "Not found");
            
            var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
            ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
            return View("DailyQueueMetricsPartial");
        }

        [HttpGet]
        [UnitOfWorkAction]
        public JsonResult QueueMetricsDetails(DateOnly date)
        {
            return Json(_myReportViewModelFactory.CreateQueueMetricsViewModel(date), JsonRequestBehavior.AllowGet);
        }
	}
}