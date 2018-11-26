using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[MyReportPermission]
	public class MyReportController : Controller
	{
		private readonly IMyReportViewModelFactory _myReportViewModelFactory;
		private readonly IUserCulture _userCulture;

        public MyReportController(IMyReportViewModelFactory myReportViewModelFactory, IUserCulture userCulture)
		{
			_myReportViewModelFactory = myReportViewModelFactory;
			_userCulture = userCulture;
		}

		[HttpGet]
		[EnsureInPortal]
		public ViewResult Index()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			return View("MyReportPartial");
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult Overview(DateOnly date)
		{
			return Json(_myReportViewModelFactory.CreateDailyMetricsViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[EnsureInPortal]
		public ViewResult Adherence()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			return View("AdherencePartial");
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult AdherenceDetails(DateOnly date)
		{
			return Json(_myReportViewModelFactory.CreateDetailedAherenceViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
        [EnsureInPortal]
		[MyReportQueueMetricsPermission]
        public ViewResult QueueMetrics()
        {
            var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
            ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
            return View("DailyQueueMetricsPartial");
        }

        [HttpGet]
        [UnitOfWork]
        public virtual JsonResult QueueMetricsDetails(DateOnly date)
        {
            return Json(_myReportViewModelFactory.CreateQueueMetricsViewModel(date), JsonRequestBehavior.AllowGet);
        }
	}
}