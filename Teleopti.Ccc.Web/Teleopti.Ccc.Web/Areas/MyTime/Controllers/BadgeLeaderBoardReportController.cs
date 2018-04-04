﻿using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class BadgeLeaderBoardReportController : Controller
    {
	    private readonly IBadgeLeaderBoardReportViewModelFactory _badgeLeaderBoardReportViewModelFactory;
	    private readonly IBadgeLeaderBoardReportOptionFactory _viewModelFactory;
	    private readonly IUserCulture _userCulture;
	    private readonly INow _now;
	    private readonly IUserTimeZone _timeZone;
		private readonly IGamificationSettingProvider _gamificationSettingProvider;

	    public BadgeLeaderBoardReportController(IBadgeLeaderBoardReportViewModelFactory badgeLeaderBoardReportViewModelFactory, IBadgeLeaderBoardReportOptionFactory viewModelFactory, IUserCulture userCulture, INow now, IUserTimeZone timeZone, IGamificationSettingProvider gamificationSettingProvider)
	    {
		    _userCulture = userCulture;
		    _now = now;
		    _timeZone = timeZone;
			_gamificationSettingProvider = gamificationSettingProvider;
			_badgeLeaderBoardReportViewModelFactory = badgeLeaderBoardReportViewModelFactory;
		    _viewModelFactory = viewModelFactory;
	    }

	    //
        // GET: /MyTime/BadgeLeaderBoardReport/

		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index()
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			var rollingPeriodSet = GamificationRollingPeriodSet.OnGoing;
			if (_gamificationSettingProvider.GetGamificationSetting() != null)
				rollingPeriodSet = _gamificationSettingProvider.GetGamificationSetting().RollingPeriodSet;
			ViewBag.DatePickerFormat = culture.DateTimeFormat.ShortDatePattern.ToUpper();
			ViewBag.GamificationRollingPeriodSet = (int)rollingPeriodSet;
			return View("BadgeLeaderBoardReportPartial");
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult Overview(LeaderboardQuery query)
		{
			return Json(_badgeLeaderBoardReportViewModelFactory.CreateBadgeLeaderBoardReportViewModel(query), JsonRequestBehavior.AllowGet);
		}

	    [UnitOfWork]
	    public virtual JsonResult OptionsForLeaderboard()
	    {
			var todayInUserTimezone = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));
			return
				Json(
					_viewModelFactory.CreateLeaderboardOptions(todayInUserTimezone, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard),
					JsonRequestBehavior.AllowGet);
	    }
    }
}
