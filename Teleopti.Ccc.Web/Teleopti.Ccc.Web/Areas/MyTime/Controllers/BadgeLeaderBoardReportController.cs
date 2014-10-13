using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class BadgeLeaderBoardReportController : Controller
    {
		private readonly IToggleManager _toggleManager;
		private readonly IBadgeLeaderBoardReportViewModelFactory _badgeLeaderBoardReportViewModelFactory;
		private readonly IUserCulture _userCulture;

	    public BadgeLeaderBoardReportController(IBadgeLeaderBoardReportViewModelFactory badgeLeaderBoardReportViewModelFactory , IUserCulture userCulture, IToggleManager toggleManager)
	    {
		    _toggleManager = toggleManager;
		    _userCulture = userCulture;
		    _badgeLeaderBoardReportViewModelFactory = badgeLeaderBoardReportViewModelFactory;
	    }

	    //
        // GET: /MyTime/BadgeLeaderBoardReport/

		[EnsureInPortal]
		public ViewResult Index()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			return View("BadgeLeaderBoardReportPartial");
		}

		[HttpGet]
		[UnitOfWorkAction]
		public JsonResult Overview(DateOnly date)
		{
			return Json(_badgeLeaderBoardReportViewModelFactory.CreateBadgeLeaderBoardReportViewModel(date), JsonRequestBehavior.AllowGet);
		}
    }
}
