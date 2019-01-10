using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class TeamController : Controller
	{
		private readonly ITeamViewModelFactory _teamViewModelFactory;
		private readonly ISiteViewModelFactory _siteViewModelFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public TeamController(ITeamViewModelFactory teamViewModelFactory, INow now, ISiteViewModelFactory siteViewModelFactory, IUserTimeZone userTimeZone)
		{
			_teamViewModelFactory = teamViewModelFactory;
			_now = now;
			_siteViewModelFactory = siteViewModelFactory;
			_userTimeZone = userTimeZone;
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult TeamsForShiftTrade(DateOnly? date)
		{
			date = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());
			return
				Json(
					_teamViewModelFactory.CreateTeamOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb).ToArray(),
					JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPost]
		public virtual JsonResult TeamsUnderSiteForShiftTrade(string siteIds, DateOnly? date)
		{
			date = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());

			var ids = siteIds.Split(',');
			if (ids[0] == "") return Json(new List<Guid>());

			var allSiteIds = ids.Select(siteId => new Guid(siteId)).ToList();
			return Json(_siteViewModelFactory.GetTeams(allSiteIds, date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb).ToArray());
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult TeamsForShiftTradeBoard(DateOnly? date)
		{
			date = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());
			return
				Json(
					_teamViewModelFactory.CreateTeamOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard).ToArray(),
					JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult SitesForShiftTrade(DateOnly? date)
		{
			date = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());
			return Json( _siteViewModelFactory.CreateSiteOptionsViewModel(date.Value, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb).ToArray(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult TeamsAndGroupsWithAllTeam(DateOnly? date)
		{
			date = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());
			return Json(
				new
				{
					teams = _teamViewModelFactory.CreateTeamOrGroupOptionsViewModel(date.Value).ToArray(),
					allTeam = new { id = "allTeams", text = Resources.AllPermittedTeamsToMakeShiftTradeWith }
				}, JsonRequestBehavior.AllowGet);
		}
	}
}