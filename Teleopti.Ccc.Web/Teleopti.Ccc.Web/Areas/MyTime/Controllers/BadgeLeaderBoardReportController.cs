using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class BadgeLeaderBoardReportController : Controller
    {
	    private readonly IBadgeLeaderBoardReportViewModelFactory _badgeLeaderBoardReportViewModelFactory;
		private readonly IUserCulture _userCulture;

	    public BadgeLeaderBoardReportController(IBadgeLeaderBoardReportViewModelFactory badgeLeaderBoardReportViewModelFactory , IUserCulture userCulture)
	    {
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
		public JsonResult Overview(LeaderboardQuery query)
		{
			return Json(_badgeLeaderBoardReportViewModelFactory.CreateBadgeLeaderBoardReportViewModel(query), JsonRequestBehavior.AllowGet);
		}
    }
}
